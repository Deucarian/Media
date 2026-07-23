using System;
using System.Threading;
using System.Threading.Tasks;

namespace Deucarian.Media
{
    public interface IMediaPlaybackSession<TOutput> : IDisposable
    {
        event EventHandler<MediaPlaybackSnapshotChangedEventArgs>
            SnapshotChanged;

        MediaPlaybackSnapshot Snapshot { get; }
        double Time { get; set; }
        double Duration { get; }

        Task<MediaPlaybackPrepareResult> PrepareAsync(
            MediaPlaybackPrepareRequest<TOutput> request,
            CancellationToken cancellationToken);

        void Play();
        void Pause();
        void Stop();
    }

    public interface IMediaPlaybackSessionFactory<TOutput>
    {
        IMediaPlaybackSession<TOutput> Create();
    }
}

