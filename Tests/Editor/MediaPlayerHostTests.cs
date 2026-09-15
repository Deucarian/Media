using System.Threading;
using System.Threading.Tasks;
using Deucarian.Media.Unity;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.Media.Tests
{
    public sealed class MediaPlayerHostTests
    {
        [Test]
        public async Task StopReleasesALateAudioResultWithoutStartingPlayback()
        {
            var go = new GameObject("player");
            var clip = AudioClip.Create("test", 10, 1, 8000, false);
            int releases = 0;
            try
            {
                var loader = new DelayedLoader();
                var player = go.AddComponent<MediaPlayerHost>();
                player.ConfigureAudio(loader, AudioType.WAV);
                var pending = player.PlayAsync("https://example.test/audio.wav");
                player.Stop();
                loader.Result.SetResult(MediaLoadResult<AudioClip>.Success(MediaResourceLease<AudioClip>.Owned(clip, _ => releases++)));
                Assert.That((await pending).Cancelled, Is.True);
                Assert.That(releases, Is.EqualTo(1));
                Assert.That(go.GetComponent<AudioSource>().clip, Is.Null);
            }
            finally { Object.DestroyImmediate(go); Object.DestroyImmediate(clip); }
        }
        private sealed class DelayedLoader : IMediaLoader<UnityAudioLoadRequest, AudioClip>
        {
            public readonly TaskCompletionSource<MediaLoadResult<AudioClip>> Result = new TaskCompletionSource<MediaLoadResult<AudioClip>>();
            public Task<MediaLoadResult<AudioClip>> LoadAsync(UnityAudioLoadRequest request, CancellationToken cancellationToken) => Result.Task;
        }
    }
}
