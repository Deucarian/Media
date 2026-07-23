using System;

namespace Deucarian.Media
{
    public enum MediaKind
    {
        Unknown,
        Image,
        Audio,
        Video,
        Text,
        Binary
    }

    public sealed class MediaSource : IEquatable<MediaSource>
    {
        public MediaSource(
            string location,
            MediaKind kind,
            string id = null,
            string contentType = null)
        {
            if (string.IsNullOrWhiteSpace(location))
            {
                throw new ArgumentException(
                    "Media source location is required.",
                    nameof(location));
            }

            Location = location.Trim();
            Kind = kind;
            Id = string.IsNullOrWhiteSpace(id) ? null : id.Trim();
            ContentType = string.IsNullOrWhiteSpace(contentType)
                ? null
                : contentType.Trim();
        }

        public string Id { get; }
        public string Location { get; }
        public string ContentType { get; }
        public MediaKind Kind { get; }

        public bool Equals(MediaSource other)
        {
            return other != null &&
                   Kind == other.Kind &&
                   string.Equals(Id, other.Id, StringComparison.Ordinal) &&
                   string.Equals(Location, other.Location, StringComparison.Ordinal) &&
                   string.Equals(ContentType, other.ContentType, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MediaSource);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)Kind;
                hash = (hash * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                hash = (hash * 397) ^ Location.GetHashCode();
                hash = (hash * 397) ^
                       (ContentType != null
                           ? StringComparer.OrdinalIgnoreCase.GetHashCode(ContentType)
                           : 0);
                return hash;
            }
        }
    }
}

