using System;

namespace Deucarian.Media
{
    public interface IMediaResourceLease<out TResource> : IDisposable
    {
        TResource Resource { get; }
        bool IsDisposed { get; }
    }

    public sealed class MediaResourceLease<TResource> :
        IMediaResourceLease<TResource>
    {
        private TResource _resource;
        private Action<TResource> _release;

        private MediaResourceLease(
            TResource resource,
            Action<TResource> release)
        {
            _resource = resource;
            _release = release;
        }

        public TResource Resource
        {
            get
            {
                if (IsDisposed)
                {
                    throw new ObjectDisposedException(GetType().Name);
                }

                return _resource;
            }
        }

        public bool IsDisposed { get; private set; }

        public static MediaResourceLease<TResource> Borrowed(
            TResource resource)
        {
            return new MediaResourceLease<TResource>(resource, null);
        }

        public static MediaResourceLease<TResource> Owned(
            TResource resource,
            Action<TResource> release)
        {
            if (release == null)
            {
                throw new ArgumentNullException(nameof(release));
            }

            return new MediaResourceLease<TResource>(resource, release);
        }

        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            Action<TResource> release = _release;
            TResource resource = _resource;
            _release = null;
            _resource = default(TResource);
            release?.Invoke(resource);
        }
    }
}

