using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Deucarian.Media
{
    public sealed class MediaLoadStrategyResolver<TRequest, TResource> :
        IMediaLoader<TRequest, TResource>
    {
        private readonly IReadOnlyList<
            IMediaLoadStrategy<TRequest, TResource>> _strategies;

        public MediaLoadStrategyResolver(
            IReadOnlyList<IMediaLoadStrategy<TRequest, TResource>> strategies)
        {
            _strategies = strategies ??
                          throw new ArgumentNullException(nameof(strategies));
        }

        public Task<MediaLoadResult<TResource>> LoadAsync(
            TRequest request,
            CancellationToken cancellationToken)
        {
            for (int i = 0; i < _strategies.Count; i++)
            {
                IMediaLoadStrategy<TRequest, TResource> strategy =
                    _strategies[i];
                if (strategy != null && strategy.CanLoad(request))
                {
                    return strategy.LoadAsync(request, cancellationToken);
                }
            }

            return Task.FromResult(
                MediaLoadResult<TResource>.Failure(
                    "No media loading strategy accepted the request."));
        }
    }
}

