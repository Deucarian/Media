using System.Threading;
using System.Threading.Tasks;

namespace Deucarian.Media
{
    public interface IMediaLoader<in TRequest, TResource>
    {
        Task<MediaLoadResult<TResource>> LoadAsync(
            TRequest request,
            CancellationToken cancellationToken);
    }

    public interface IMediaLoadStrategy<in TRequest, TResource> :
        IMediaLoader<TRequest, TResource>
    {
        bool CanLoad(TRequest request);
    }
}

