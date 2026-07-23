using NUnit.Framework;

namespace Deucarian.Media.Tests
{
    public sealed class MediaResourceLeaseTests
    {
        [Test]
        public void OwnedLeaseReleasesExactlyOnce()
        {
            int releases = 0;
            var lease = MediaResourceLease<string>.Owned(
                "resource",
                _ => releases++);

            Assert.AreEqual("resource", lease.Resource);
            lease.Dispose();
            lease.Dispose();

            Assert.AreEqual(1, releases);
            Assert.IsTrue(lease.IsDisposed);
        }

        [Test]
        public void LoadResultDisposesOwnedLease()
        {
            int releases = 0;
            MediaLoadResult<string> result =
                MediaLoadResult<string>.Success(
                    MediaResourceLease<string>.Owned(
                        "resource",
                        _ => releases++));

            result.Dispose();
            result.Dispose();

            Assert.AreEqual(1, releases);
        }
    }
}

