using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Deucarian.Media.Tests
{
    public sealed class MediaLoadStrategyResolverTests
    {
        [Test]
        public async Task FirstAcceptingStrategyHandlesRequest()
        {
            var rejected = new RecordingStrategy(false, "rejected");
            var accepted = new RecordingStrategy(true, "accepted");
            var resolver =
                new MediaLoadStrategyResolver<string, string>(
                    new[] { rejected, accepted });

            MediaLoadResult<string> result =
                await resolver.LoadAsync(
                    "request",
                    CancellationToken.None);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(
                "accepted",
                result.ResourceLease.Resource);
            Assert.AreEqual(0, rejected.LoadCount);
            Assert.AreEqual(1, accepted.LoadCount);
            result.Dispose();
        }

        [Test]
        public async Task MissingStrategyReturnsFailure()
        {
            var resolver =
                new MediaLoadStrategyResolver<string, string>(
                    new[]
                    {
                        new RecordingStrategy(false, "unused")
                    });

            MediaLoadResult<string> result =
                await resolver.LoadAsync(
                    "request",
                    CancellationToken.None);

            Assert.AreEqual(MediaLoadStatus.Failed, result.Status);
        }

        private sealed class RecordingStrategy :
            IMediaLoadStrategy<string, string>
        {
            private readonly bool _accepts;
            private readonly string _resource;

            public RecordingStrategy(
                bool accepts,
                string resource)
            {
                _accepts = accepts;
                _resource = resource;
            }

            public int LoadCount { get; private set; }

            public bool CanLoad(string request)
            {
                return _accepts;
            }

            public Task<MediaLoadResult<string>> LoadAsync(
                string request,
                CancellationToken cancellationToken)
            {
                LoadCount++;
                return Task.FromResult(
                    MediaLoadResult<string>.Success(
                        MediaResourceLease<string>.Borrowed(
                            _resource)));
            }
        }
    }
}

