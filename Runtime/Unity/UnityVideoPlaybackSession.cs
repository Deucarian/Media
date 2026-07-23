using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Deucarian.Common;
using UnityEngine;
using UnityEngine.Video;

namespace Deucarian.Media.Unity
{
    public interface IUnityVideoPlaybackSessionFactory
    {
        IMediaPlaybackSession<RenderTexture> Create(GameObject host);
    }

    public sealed class UnityVideoPlaybackSessionFactory :
        IUnityVideoPlaybackSessionFactory
    {
        public IMediaPlaybackSession<RenderTexture> Create(
            GameObject host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            VideoPlayer player = host.GetComponent<VideoPlayer>();
            bool ownsPlayer = player == null;
            if (player == null)
            {
                player = host.AddComponent<VideoPlayer>();
            }

            return new UnityVideoPlaybackSession(
                player,
                ownsPlayer);
        }
    }

    public sealed class UnityVideoPlaybackSession :
        IMediaPlaybackSession<RenderTexture>
    {
        // VideoPlayer.httpHeaders is not available on every supported Unity
        // version. Reflection is isolated here as the compatibility boundary.
        private static readonly PropertyInfo HttpHeadersProperty =
            typeof(VideoPlayer).GetProperty(
                "httpHeaders",
                BindingFlags.Instance | BindingFlags.Public);

        private readonly VideoPlayer _player;
        private readonly bool _ownsPlayer;
        private TaskCompletionSource<MediaPlaybackPrepareResult>
            _prepareCompletion;
        private CancellationTokenRegistration
            _prepareCancellationRegistration;
        private MediaPlaybackState _state;
        private bool _isDisposed;

        public UnityVideoPlaybackSession(
            VideoPlayer player,
            bool ownsPlayer = false)
        {
            _player = player ??
                      throw new ArgumentNullException(nameof(player));
            _ownsPlayer = ownsPlayer;
            _state = MediaPlaybackState.Idle;
        }

        public event EventHandler<
            MediaPlaybackSnapshotChangedEventArgs> SnapshotChanged;

        public MediaPlaybackSnapshot Snapshot =>
            new MediaPlaybackSnapshot(
                _state,
                Time,
                Duration);

        public double Time
        {
            get => IsPlayerAlive
                ? Math.Max(0d, _player.time)
                : 0d;
            set
            {
                ThrowIfDisposed();
                if (!IsPlayerAlive)
                {
                    return;
                }

                MediaPlaybackSnapshot previous = Snapshot;
                _player.time = Math.Max(0d, value);
                RaiseSnapshotChanged(previous);
            }
        }

        public double Duration =>
            IsPlayerAlive
                ? Math.Max(0d, _player.length)
                : 0d;

        private bool IsPlayerAlive =>
            _player != null &&
            _player.gameObject != null;

        public Task<MediaPlaybackPrepareResult> PrepareAsync(
            MediaPlaybackPrepareRequest<RenderTexture> request,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            if (request == null)
            {
                return Task.FromResult(
                    MediaPlaybackPrepareResult.Failure(
                        "Video playback request is required."));
            }

            if (request.Source.Kind != MediaKind.Video)
            {
                return Task.FromResult(
                    MediaPlaybackPrepareResult.Failure(
                        "Video playback requires a video media source."));
            }

            if (request.Output == null)
            {
                return Task.FromResult(
                    MediaPlaybackPrepareResult.Failure(
                        "Video playback requires a RenderTexture output."));
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return Task.FromResult(
                    MediaPlaybackPrepareResult.CancelledResult());
            }

            CancelPrepare();
            ConfigurePlayer(request);
            SetState(MediaPlaybackState.Preparing);
            _prepareCompletion =
                new TaskCompletionSource<MediaPlaybackPrepareResult>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
            _player.prepareCompleted += OnPrepareCompleted;
            _player.errorReceived += OnErrorReceived;
            _prepareCancellationRegistration =
                cancellationToken.Register(OnPrepareCancelled);

            try
            {
                _player.Prepare();
                return _prepareCompletion.Task;
            }
            catch (Exception exception)
            {
                ResetPrepareHandlers();
                SetState(MediaPlaybackState.Failed);
                return Task.FromResult(
                    MediaPlaybackPrepareResult.Failure(
                        exception.Message));
            }
        }

        public void Play()
        {
            ThrowIfDisposed();
            if (!CanControlPlayer || !_player.isPrepared)
            {
                return;
            }

            _player.Play();
            SetState(MediaPlaybackState.Playing);
        }

        public void Pause()
        {
            ThrowIfDisposed();
            if (!CanControlPlayer)
            {
                return;
            }

            _player.Pause();
            SetState(MediaPlaybackState.Paused);
        }

        public void Stop()
        {
            ThrowIfDisposed();
            if (!CanControlPlayer)
            {
                return;
            }

            _player.Stop();
            SetState(MediaPlaybackState.Stopped);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            CancelPrepare();
            if (CanControlPlayer)
            {
                _player.Stop();
            }

            MediaPlaybackSnapshot previous = Snapshot;
            _isDisposed = true;
            _state = MediaPlaybackState.Disposed;
            RaiseSnapshotChanged(previous);

            if (_ownsPlayer)
            {
                UnityObjectUtility.DestroySafely(_player);
            }
        }

        private bool CanControlPlayer =>
            IsPlayerAlive &&
            _player.isActiveAndEnabled &&
            _player.gameObject.activeInHierarchy;

        private void ConfigurePlayer(
            MediaPlaybackPrepareRequest<RenderTexture> request)
        {
            _player.playOnAwake = false;
            _player.isLooping = false;
            _player.source = VideoSource.Url;
            _player.url = request.Source.Location;
            _player.renderMode = VideoRenderMode.RenderTexture;
            _player.targetTexture = request.Output;
            _player.audioOutputMode =
                VideoAudioOutputMode.Direct;
            TryApplyHeaders(_player, request.Headers);
        }

        private void OnPrepareCompleted(VideoPlayer source)
        {
            if (source != _player)
            {
                return;
            }

            TaskCompletionSource<MediaPlaybackPrepareResult> completion =
                _prepareCompletion;
            ResetPrepareHandlers();
            SetState(MediaPlaybackState.Ready);
            completion?.TrySetResult(
                MediaPlaybackPrepareResult.Success(
                    SafeDimension(_player.width),
                    SafeDimension(_player.height),
                    _player.length));
        }

        private void OnErrorReceived(
            VideoPlayer source,
            string message)
        {
            if (source != _player)
            {
                return;
            }

            TaskCompletionSource<MediaPlaybackPrepareResult> completion =
                _prepareCompletion;
            ResetPrepareHandlers();
            SetState(MediaPlaybackState.Failed);
            completion?.TrySetResult(
                MediaPlaybackPrepareResult.Failure(
                    string.IsNullOrWhiteSpace(message)
                        ? "Video failed to load."
                        : message));
        }

        private void OnPrepareCancelled()
        {
            TaskCompletionSource<MediaPlaybackPrepareResult> completion =
                _prepareCompletion;
            ResetPrepareHandlers();
            SetState(MediaPlaybackState.Idle);
            completion?.TrySetResult(
                MediaPlaybackPrepareResult.CancelledResult());
        }

        private void CancelPrepare()
        {
            TaskCompletionSource<MediaPlaybackPrepareResult> completion =
                _prepareCompletion;
            ResetPrepareHandlers();
            completion?.TrySetResult(
                MediaPlaybackPrepareResult.CancelledResult());
        }

        private void ResetPrepareHandlers()
        {
            _prepareCancellationRegistration.Dispose();
            if (_player != null)
            {
                _player.prepareCompleted -= OnPrepareCompleted;
                _player.errorReceived -= OnErrorReceived;
            }

            _prepareCompletion = null;
        }

        private void SetState(MediaPlaybackState state)
        {
            if (_state == state)
            {
                return;
            }

            MediaPlaybackSnapshot previous = Snapshot;
            _state = state;
            RaiseSnapshotChanged(previous);
        }

        private void RaiseSnapshotChanged(
            MediaPlaybackSnapshot previous)
        {
            SnapshotChanged?.Invoke(
                this,
                new MediaPlaybackSnapshotChangedEventArgs(
                    previous,
                    Snapshot));
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private static int SafeDimension(uint value)
        {
            return value > int.MaxValue
                ? int.MaxValue
                : (int)value;
        }

        private static bool TryApplyHeaders(
            VideoPlayer player,
            IReadOnlyDictionary<string, string> headers)
        {
            if (player == null ||
                headers == null ||
                headers.Count == 0 ||
                HttpHeadersProperty == null ||
                !HttpHeadersProperty.CanWrite)
            {
                return false;
            }

            try
            {
                var compatibleHeaders =
                    new Dictionary<string, string>();
                foreach (KeyValuePair<string, string> header in headers)
                {
                    compatibleHeaders[header.Key] = header.Value;
                }

                HttpHeadersProperty.SetValue(
                    player,
                    compatibleHeaders);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
