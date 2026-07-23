using System.Threading;
using System.Threading.Tasks;
using Deucarian.Media.Unity;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Video;

namespace Deucarian.Media.Tests
{
    public sealed class UnityVideoPlaybackSessionTests
    {
        [Test]
        public async Task PrepareRejectsNonVideoSource()
        {
            var host = new GameObject("MediaVideoSessionTest");
            VideoPlayer player = host.AddComponent<VideoPlayer>();
            var session =
                new UnityVideoPlaybackSession(player);
            var target = new RenderTexture(16, 16, 0);

            MediaPlaybackPrepareResult result =
                await session.PrepareAsync(
                    new MediaPlaybackPrepareRequest<RenderTexture>(
                        new MediaSource(
                            "https://example.test/image.png",
                            MediaKind.Image),
                        target),
                    CancellationToken.None);

            Assert.IsFalse(result.Succeeded);
            Assert.IsFalse(result.Cancelled);

            session.Dispose();
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void OwnedPlayerIsDestroyedOnDispose()
        {
            var host = new GameObject("OwnedMediaVideoSessionTest");
            VideoPlayer player = host.AddComponent<VideoPlayer>();
            var session =
                new UnityVideoPlaybackSession(player, true);

            session.Dispose();

            Assert.IsTrue(player == null);
            Object.DestroyImmediate(host);
        }
    }
}

