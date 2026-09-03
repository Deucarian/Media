using UnityEngine;

namespace Deucarian.Media.Unity
{
    /// <summary>Minimal Unity output boundary for short overlapping UI feedback cues.</summary>
    public interface IUnityAudioOneShotOutput
    {
        bool TryPlay(AudioClip clip, float volume = 1f, float pitch = 1f);

        void StopAll();
    }
}
