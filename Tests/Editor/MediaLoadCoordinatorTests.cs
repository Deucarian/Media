using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Deucarian.Media.Tests
{
    public sealed class MediaLoadCoordinatorTests
    {
        [Test]
        public async Task NewRequestMakesPreviousResultStaleAndDisposesIt()
        {
            var loader = new ControlledLoader();
            var coordinator =
                new MediaLoadCoordinator<string, string>(loader);

            Task<MediaLoadResult<string>> firstTask =
                coordinator.LoadLatestAsync(
                    "first",
                    CancellationToken.None);
            Task<MediaLoadResult<string>> secondTask =
                coordinator.LoadLatestAsync(
                    "second",
                    CancellationToken.None);

            loader.Complete(
                "first",
                MediaResourceLease<string>.Owned(
                    "old",
                    _ => loader.ReleaseCount++));
            loader.Complete(
                "second",
                MediaResourceLease<string>.Borrowed("new"));

            MediaLoadResult<string> first = await firstTask;
            MediaLoadResult<string> second = await secondTask;

            Assert.AreEqual(MediaLoadStatus.Stale, first.Status);
            Assert.AreEqual(1, loader.ReleaseCount);
            Assert.IsTrue(second.Succeeded);
            Assert.AreEqual("new", second.ResourceLease.Resource);

            second.Dispose();
            coordinator.Dispose();
        }

        private sealed class ControlledLoader :
            IMediaLoader<string, string>
        {
            private readonly Dictionary<
                string,
                TaskCompletionSource<MediaLoadResult<string>>> _pending =
                    new Dictionary<
                        string,
                        TaskCompletionSource<MediaLoadResult<string>>>();

            public int ReleaseCount { get; set; }

            public Task<MediaLoadResult<string>> LoadAsync(
                string request,
                CancellationToken cancellationToken)
            {
                var completion =
                    new TaskCompletionSource<MediaLoadResult<string>>(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                _pending[request] = completion;
                return completion.Task;
            }

            public void Complete(
                string request,
                IMediaResourceLease<string> lease)
            {
                _pending[request].TrySetResult(
                    MediaLoadResult<string>.Success(lease));
            }
        }
    }
}

