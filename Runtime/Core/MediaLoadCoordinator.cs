using System;
using System.Threading;
using System.Threading.Tasks;

namespace Deucarian.Media
{
    public sealed class MediaLoadCoordinator<TRequest, TResource> :
        IDisposable
    {
        private readonly IMediaLoader<TRequest, TResource> _loader;
        private CancellationTokenSource _activeRequest;
        private int _version;
        private bool _isDisposed;

        public MediaLoadCoordinator(
            IMediaLoader<TRequest, TResource> loader)
        {
            _loader = loader ??
                      throw new ArgumentNullException(nameof(loader));
        }

        public async Task<MediaLoadResult<TResource>> LoadLatestAsync(
            TRequest request,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
            CancelActiveRequest();
            int requestVersion = ++_version;
            _activeRequest =
                CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationToken);
            CancellationToken requestToken = _activeRequest.Token;

            MediaLoadResult<TResource> result;
            try
            {
                result = await _loader.LoadAsync(
                    request,
                    requestToken);
            }
            catch (OperationCanceledException)
            {
                result = MediaLoadResult<TResource>.Cancelled();
            }
            catch (Exception exception)
            {
                result = MediaLoadResult<TResource>.Failure(
                    exception.Message);
            }

            if (_isDisposed ||
                requestVersion != _version ||
                requestToken.IsCancellationRequested)
            {
                result?.Dispose();
                return cancellationToken.IsCancellationRequested
                    ? MediaLoadResult<TResource>.Cancelled()
                    : MediaLoadResult<TResource>.Stale();
            }

            return result ??
                   MediaLoadResult<TResource>.Failure(
                       "Media loader returned no result.");
        }

        public void Cancel()
        {
            ThrowIfDisposed();
            ++_version;
            CancelActiveRequest();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
            ++_version;
            CancelActiveRequest();
        }

        private void CancelActiveRequest()
        {
            CancellationTokenSource active = _activeRequest;
            _activeRequest = null;
            if (active == null)
            {
                return;
            }

            active.Cancel();
            active.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }
    }
}

