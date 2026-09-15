using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Deucarian.Media
{
    /// <summary>
    /// A caller-owned preparation lifetime. Work is coalesced by key, bounded, and
    /// starts only after the injected frame/budget gate. Call from one owning context.
    /// </summary>
    public sealed class MediaPreparationQueue<TKey> : IDisposable
    {
        private sealed class Work
        {
            public TKey Key;
            public Func<CancellationToken, Task> Prepare;
            public CancellationTokenSource Attempt;
            public readonly TaskCompletionSource<bool> Completion =
                new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private readonly Func<CancellationToken, Task> nextStep;
        private readonly int concurrency;
        private readonly LinkedList<Work> pending = new LinkedList<Work>();
        private readonly Dictionary<TKey, Work> requests = new Dictionary<TKey, Work>();
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private int workers;
        private bool disposed;
        private bool hasPriority;
        private TKey priorityKey;

        public MediaPreparationQueue(Func<CancellationToken, Task> nextStep, int concurrency = 1)
        {
            this.nextStep = nextStep ?? throw new ArgumentNullException(nameof(nextStep));
            if (concurrency < 1) throw new ArgumentOutOfRangeException(nameof(concurrency));
            this.concurrency = concurrency;
        }

        public Task Enqueue(TKey key, Func<CancellationToken, Task> prepare, bool priority = false)
        {
            if (disposed) throw new ObjectDisposedException(nameof(MediaPreparationQueue<TKey>));
            if (prepare == null) throw new ArgumentNullException(nameof(prepare));
            if (requests.TryGetValue(key, out Work existing))
            {
                if (priority && pending.Remove(existing)) pending.AddFirst(existing);
                return existing.Completion.Task;
            }
            var work = new Work { Key = key, Prepare = prepare };
            requests.Add(key, work);
            if (priority || (hasPriority && EqualityComparer<TKey>.Default.Equals(key, priorityKey)))
                pending.AddFirst(work);
            else pending.AddLast(work);
            if (workers < concurrency)
            {
                workers++;
                _ = RunAsync();
            }
            return work.Completion.Task;
        }

        public void Prioritize(TKey key) => Prioritize(key, false);

        /// <summary>Interrupt other attempts and pause background work until ClearPriority when requested.</summary>
        public void Prioritize(TKey key, bool interruptRunning)
        {
            if (!disposed && requests.TryGetValue(key, out Work work) && pending.Remove(work))
                pending.AddFirst(work);
            if (disposed || !interruptRunning) return;
            hasPriority = true;
            priorityKey = key;
            // Cancel the current attempt, not its request. It can resume after the
            // selected item, and callers keep the same coalesced completion task.
            foreach (Work current in new List<Work>(requests.Values))
                if (!EqualityComparer<TKey>.Default.Equals(current.Key, key))
                    current.Attempt?.Cancel();
            StartWorkerIfNeeded();
        }

        public void ClearPriority()
        {
            hasPriority = false;
            StartWorkerIfNeeded();
        }

        private void StartWorkerIfNeeded()
        {
            if (disposed || pending.Count == 0 || workers >= concurrency) return;
            workers++;
            _ = RunAsync();
        }

        private async Task RunAsync()
        {
            CancellationToken token = lifetime.Token;
            try
            {
                while (!disposed && pending.Count > 0)
                {
                    Work work = null;
                    bool retry = false;
                    try
                    {
                        await nextStep(token);
                        token.ThrowIfCancellationRequested();
                        if (pending.Count == 0) break;
                        if (hasPriority && !EqualityComparer<TKey>.Default.Equals(pending.First.Value.Key, priorityKey)) break;
                        work = pending.First.Value;
                        pending.RemoveFirst();
                        work.Attempt = CancellationTokenSource.CreateLinkedTokenSource(token);
                        await work.Prepare(work.Attempt.Token);
                        work.Attempt.Token.ThrowIfCancellationRequested();
                        token.ThrowIfCancellationRequested();
                        work.Completion.TrySetResult(true);
                    }
                    catch (OperationCanceledException)
                    {
                        retry = work?.Attempt?.IsCancellationRequested == true && !token.IsCancellationRequested && !disposed;
                        if (retry) pending.AddLast(work);
                        else work?.Completion.TrySetCanceled();
                        if (token.IsCancellationRequested) break;
                    }
                    catch (Exception exception)
                    {
                        if (work == null)
                        {
                            foreach (Work queued in pending)
                            {
                                requests.Remove(queued.Key);
                                queued.Completion.TrySetException(exception);
                            }
                            pending.Clear();
                            break;
                        }
                        work.Completion.TrySetException(exception);
                    }
                    finally
                    {
                        if (work != null)
                        {
                            work.Attempt?.Dispose();
                            work.Attempt = null;
                            if (!retry) requests.Remove(work.Key);
                        }
                    }
                }
            }
            finally
            {
                workers--;
                if (disposed && workers == 0) lifetime.Dispose();
            }
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            lifetime.Cancel();
            foreach (Work work in pending) work.Completion.TrySetCanceled();
            pending.Clear();
            requests.Clear();
            if (workers == 0) lifetime.Dispose();
        }
    }
}
