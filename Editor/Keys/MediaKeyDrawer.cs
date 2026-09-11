using System;
using Deucarian.Media.Unity;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.Media.Editor
{
    [CustomPropertyDrawer(typeof(MediaKey), true)]
    public sealed class MediaKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(MediaKey);
        public override Type DefinitionSetAttribute => typeof(MediaKeySetAttribute);
        public override string SetupHint => "Select an existing MediaKey; declare reusable keys once in a [MediaKeySet] class.";
    }
}
