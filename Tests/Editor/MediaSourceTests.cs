using System;
using NUnit.Framework;

namespace Deucarian.Media.Tests
{
    public sealed class MediaSourceTests
    {
        [Test]
        public void ConstructorNormalizesOptionalMetadata()
        {
            var source = new MediaSource(
                " https://example.test/image.png ",
                MediaKind.Image,
                " image-1 ",
                " image/png ");

            Assert.AreEqual(
                "https://example.test/image.png",
                source.Location);
            Assert.AreEqual("image-1", source.Id);
            Assert.AreEqual("image/png", source.ContentType);
            Assert.AreEqual(MediaKind.Image, source.Kind);
        }

        [Test]
        public void EmptyLocationIsRejected()
        {
            Assert.Throws<ArgumentException>(
                () => new MediaSource(" ", MediaKind.Text));
        }

        [Test]
        public void EquivalentSourcesCompareEqual()
        {
            var first = new MediaSource(
                "https://example.test/data.txt",
                MediaKind.Text,
                "text-1",
                "text/plain");
            var second = new MediaSource(
                "https://example.test/data.txt",
                MediaKind.Text,
                "text-1",
                "TEXT/PLAIN");

            Assert.AreEqual(first, second);
            Assert.AreEqual(
                first.GetHashCode(),
                second.GetHashCode());
        }
    }
}

