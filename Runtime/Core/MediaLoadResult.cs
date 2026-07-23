using System;

namespace Deucarian.Media
{
    public enum MediaLoadStatus
    {
        Succeeded,
        Failed,
        Cancelled,
        Stale
    }

    public sealed class MediaLoadResult<TResource> : IDisposable
    {
        private MediaLoadResult(
            MediaLoadStatus status,
            IMediaResourceLease<TResource> resourceLease,
            string error)
        {
            Status = status;
            ResourceLease = resourceLease;
            Error = error;
        }

        public MediaLoadStatus Status { get; }
        public IMediaResourceLease<TResource> ResourceLease { get; }
        public string Error { get; }
        public bool Succeeded => Status == MediaLoadStatus.Succeeded;
        public bool IsCancelled => Status == MediaLoadStatus.Cancelled;
        public bool IsStale => Status == MediaLoadStatus.Stale;

        public static MediaLoadResult<TResource> Success(
            IMediaResourceLease<TResource> resourceLease)
        {
            if (resourceLease == null)
            {
                throw new ArgumentNullException(nameof(resourceLease));
            }

            return new MediaLoadResult<TResource>(
                MediaLoadStatus.Succeeded,
                resourceLease,
                null);
        }

        public static MediaLoadResult<TResource> Failure(string error)
        {
            return new MediaLoadResult<TResource>(
                MediaLoadStatus.Failed,
                null,
                string.IsNullOrWhiteSpace(error)
                    ? "Media load failed."
                    : error.Trim());
        }

        public static MediaLoadResult<TResource> Cancelled()
        {
            return new MediaLoadResult<TResource>(
                MediaLoadStatus.Cancelled,
                null,
                null);
        }

        public static MediaLoadResult<TResource> Stale()
        {
            return new MediaLoadResult<TResource>(
                MediaLoadStatus.Stale,
                null,
                null);
        }

        public void Dispose()
        {
            ResourceLease?.Dispose();
        }
    }
}

