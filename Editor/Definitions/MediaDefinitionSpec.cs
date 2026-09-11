using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.Media.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.Media.Editor.Definitions
{
    [Serializable]
    public sealed class MediaDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("kind")] public MediaKind Kind = MediaKind.Audio;
        [DefinitionField("clip")] public AudioClip Clip = null;
        [DefinitionField("url")] public string Url = string.Empty;
        [DefinitionField("audioType")] public AudioType AudioType = AudioType.UNKNOWN;
        [DefinitionField("loop")] public bool Loop = false;
        [DefinitionField("volume")] public float Volume = 1f;
    }
}
