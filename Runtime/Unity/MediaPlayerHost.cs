using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deucarian.Media.Unity
{
    /// <summary>Owns one audio or video playback slot. Call from Unity's main thread.</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public sealed class MediaPlayerHost : MonoBehaviour
    {
        [SerializeField] private MediaKind kind = MediaKind.Audio;
        [SerializeField] private AudioType audioType = AudioType.UNKNOWN;
        [SerializeField] private RenderTexture videoOutput;
        private IMediaLoader<UnityAudioLoadRequest, AudioClip> audioLoader = new UnityAudioClipMediaLoader();
        private IMediaPlaybackSession<RenderTexture> video;
        private MediaLoadResult<AudioClip> audioResult;
        private CancellationTokenSource pending;
        private int generation;

        /// <summary>Optional explicit adapter composition, before playback starts.</summary>
        public void ConfigureAudio(IMediaLoader<UnityAudioLoadRequest, AudioClip> loader, AudioType type)
        {
            if (loader == null) throw new ArgumentNullException(nameof(loader));
            Stop();
            audioLoader = loader;
            audioType = type;
            kind = MediaKind.Audio;
        }

        public void ConfigureVideo(RenderTexture output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            Stop();
            videoOutput = output;
            kind = MediaKind.Video;
        }

        public async Task<MediaPlaybackPrepareResult> PlayAsync(string url, CancellationToken cancellationToken = default)
        {
            if (!isActiveAndEnabled) throw new InvalidOperationException("The media player must be enabled.");
            var source = new MediaSource(url, kind);
            Stop();
            int operation = generation;
            var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            pending = cancellation;
            try
            {
                if (cancellation.IsCancellationRequested) return MediaPlaybackPrepareResult.CancelledResult();
                if (kind == MediaKind.Video)
                {
                    if (videoOutput == null) return MediaPlaybackPrepareResult.Failure("Assign a video RenderTexture output.");
                    video = video ?? new UnityVideoPlaybackSessionFactory().Create(gameObject);
                    var result = await video.PrepareAsync(new MediaPlaybackPrepareRequest<RenderTexture>(source, videoOutput), cancellation.Token);
                    if (operation != generation || cancellation.IsCancellationRequested) return MediaPlaybackPrepareResult.CancelledResult();
                    if (result.Succeeded) video.Play();
                    return result;
                }
                if (kind != MediaKind.Audio) return MediaPlaybackPrepareResult.Failure("The player supports audio and video sources.");
                var loaded = await audioLoader.LoadAsync(new UnityAudioLoadRequest(source, audioType), cancellation.Token);
                if (loaded == null) return MediaPlaybackPrepareResult.Failure("The audio loader returned no result.");
                if (operation != generation || cancellation.IsCancellationRequested || loaded.IsCancelled || loaded.IsStale)
                {
                    loaded.Dispose();
                    return MediaPlaybackPrepareResult.CancelledResult();
                }
                if (!loaded.Succeeded)
                {
                    loaded.Dispose();
                    return MediaPlaybackPrepareResult.Failure(loaded.Error);
                }
                audioResult = loaded;
                var output = GetComponent<AudioSource>();
                output.clip = loaded.ResourceLease.Resource;
                output.Play();
                return MediaPlaybackPrepareResult.Success(0, 0, output.clip.length);
            }
            catch (OperationCanceledException) { return MediaPlaybackPrepareResult.CancelledResult(); }
            finally
            {
                if (ReferenceEquals(pending, cancellation)) pending = null;
                cancellation.Dispose();
            }
        }

        public void Stop()
        {
            generation++;
            var cancellation = pending;
            pending = null;
            cancellation?.Cancel();
            video?.Stop();
            var output = GetComponent<AudioSource>();
            if (output != null && audioResult != null) { output.Stop(); output.clip = null; }
            audioResult?.Dispose();
            audioResult = null;
        }

        private void OnDisable() { Stop(); video?.Dispose(); video = null; }
    }
}
