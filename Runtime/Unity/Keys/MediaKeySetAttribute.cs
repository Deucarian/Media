using System;

namespace Deucarian.Media.Unity
{
    /// <summary>Marks an authoritative set of named MediaKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MediaKeySetAttribute : Attribute { }
}
