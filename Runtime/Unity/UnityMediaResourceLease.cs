using Deucarian.Common;
using UnityEngine;

namespace Deucarian.Media.Unity
{
    public static class UnityMediaResourceLease
    {
        public static IMediaResourceLease<TResource> CreateOwned<TResource>(
            TResource resource)
            where TResource : Object
        {
            return MediaResourceLease<TResource>.Owned(
                resource,
                UnityObjectUtility.DestroySafely);
        }
    }
}

