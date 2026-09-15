using System;

using UnityEngine;

namespace Deucarian.Media.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class MediaDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private MediaKind kind = MediaKind.Audio;
        [SerializeField] private AudioClip clip = null;
        [SerializeField] private string url = string.Empty;
        [SerializeField] private AudioType audioType = AudioType.UNKNOWN;
        [SerializeField] private bool loop = false;
        [SerializeField] private float volume = 1f;
        public string Id => id;
        public string DisplayName => displayName;
        public MediaKey Key => new AssetKey(id);
        public MediaKind Kind => kind;
        public AudioClip Clip => clip;
        public string Url => url;
        public AudioType AudioType => audioType;
        public bool Loop => loop;
        public float Volume => volume;
        public void Validate()
        {
            if (kind != MediaKind.Audio && kind != MediaKind.Video) throw new InvalidOperationException("Select Audio or Video in the media definition.");
            if (clip == null && string.IsNullOrWhiteSpace(url)) throw new InvalidOperationException("Assign an audio clip or media URL in Definitions before playing it.");
            if (clip != null && (kind != MediaKind.Audio || !string.IsNullOrWhiteSpace(url))) throw new InvalidOperationException("Use either an Audio clip or a URL on this definition.");
            if (float.IsNaN(volume) || volume < 0f || volume > 1f) throw new InvalidOperationException("Set media volume between zero and one.");
        }
        private sealed class AssetKey : MediaKey { public AssetKey(string value) : base(value) { } }
    }
}
