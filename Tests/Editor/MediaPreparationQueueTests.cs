using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Deucarian.Media.Tests
{
    public sealed class MediaPreparationQueueTests
    {
        [Test]
        public async Task SelectedItemInterruptsOldAttemptAndBackgroundWaitsUntilSelectionClears()
        {
            var oldAttempt = new TaskCompletionSource<bool>();
            var order = new List<string>();
            CancellationToken firstToken = default;
            int attempts = 0;
            using (var queue = new MediaPreparationQueue<string>(_ => Task.CompletedTask))
            {
                Task old = queue.Enqueue("old", token =>
                {
                    firstToken = token;
                    order.Add("old-" + ++attempts);
                    return attempts == 1 ? oldAttempt.Task : Task.CompletedTask;
                });
                Task selected = queue.Enqueue("selected", _ => { order.Add("selected"); return Task.CompletedTask; });
                Task background = queue.Enqueue("background", _ => { order.Add("background"); return Task.CompletedTask; });
                queue.Prioritize("selected", true);
                Assert.True(firstToken.IsCancellationRequested);
                oldAttempt.SetResult(true); // Even a transport finishing late cannot complete the old request.
                await selected;
                Assert.False(old.IsCompleted);
                Assert.False(background.IsCompleted);
                Assert.That(order, Is.EqualTo(new[] { "old-1", "selected" }));
                queue.ClearPriority();
                await Task.WhenAll(old, background);
                Assert.That(attempts, Is.EqualTo(2));
            }
        }

        [Test]
        public async Task FocusCanPrecedeEnqueueAndUnrelatedCancellationIsNotRetried()
        {
            using (var queue = new MediaPreparationQueue<string>(_ => Task.CompletedTask))
            {
                queue.Prioritize("selected", true);
                Task waiting = queue.Enqueue("background", _ => Task.CompletedTask);
                await queue.Enqueue("selected", _ => Task.CompletedTask);
                Assert.False(waiting.IsCompleted);
                queue.ClearPriority();
                await waiting;
                int calls = 0;
                Task cancelled = queue.Enqueue("cancelled", _ =>
                { calls++; throw new System.OperationCanceledException(); });
                try { await cancelled; Assert.Fail(); } catch (System.OperationCanceledException) { }
                Assert.That(calls, Is.EqualTo(1));
            }
        }

        [Test]
        public async Task WorkWaitsForBudgetCoalescesAndPromotesSelection()
        {
            var gate = new TaskCompletionSource<bool>();
            var order = new List<string>();
            using (var queue = new MediaPreparationQueue<string>(_ => gate.Task))
            {
                Task first = queue.Enqueue("first", _ => { order.Add("first"); return Task.CompletedTask; });
                Task second = queue.Enqueue("second", _ => { order.Add("second"); return Task.CompletedTask; });
                Task promoted = queue.Enqueue("second", _ => { Assert.Fail("Duplicate work ran"); return Task.CompletedTask; }, true);
                Assert.That(promoted, Is.SameAs(second));
                Assert.That(order, Is.Empty);
                gate.SetResult(true);
                await Task.WhenAll(first, second);
                Assert.That(order, Is.EqualTo(new[] { "second", "first" }));
            }
        }

        [Test]
        public async Task CancelledLifetimeCancelsRunningWorkAndNeverStartsQueuedWork()
        {
            var finish = new TaskCompletionSource<bool>();
            CancellationToken running = default;
            var queue = new MediaPreparationQueue<string>(_ => Task.CompletedTask);
            Task first = queue.Enqueue("first", token => { running = token; return finish.Task; });
            Task second = queue.Enqueue("second", _ => { Assert.Fail("Stale work started"); return Task.CompletedTask; });
            queue.Dispose();
            Assert.That(running.IsCancellationRequested, Is.True);
            Assert.That(second.IsCanceled, Is.True);
            finish.SetResult(true);
            try { await first; Assert.Fail("Late completion was accepted"); }
            catch (System.OperationCanceledException) { }
            queue.Dispose();
        }

        [Test]
        public async Task FailedPreparationDoesNotPreventNextItem()
        {
            using (var queue = new MediaPreparationQueue<string>(_ => Task.CompletedTask))
            {
                Task failed = queue.Enqueue("bad", _ => Task.FromException(new System.InvalidOperationException()));
                try { await failed; Assert.Fail(); } catch (System.InvalidOperationException) { }
                bool prepared = false;
                await queue.Enqueue("good", _ => { prepared = true; return Task.CompletedTask; });
                Assert.That(prepared, Is.True);
            }
        }
    }
}
