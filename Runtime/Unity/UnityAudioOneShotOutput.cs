using System.Collections.Generic;
using Deucarian.Common;
using UnityEngine;

namespace Deucarian.Media.Unity
{
    /// <summary>Small pooled Unity adapter for short overlapping UI sounds with per-cue pitch.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AudioSource))]
    public sealed class UnityAudioOneShotOutput : MonoBehaviour, IUnityAudioOneShotOutput
    {
        [SerializeField] private AudioSource template;
        [SerializeField, Min(1)] private int voiceCount = 8;

        private readonly List<AudioSource> voices = new List<AudioSource>();
        private int nextVoice;

        public AudioSource Template
        {
            get => template;
            set
            {
                if (template == value) return;
                StopAll();
                ReleaseVoices();
                template = value;
            }
        }

        public int VoiceCount
        {
            get => Mathf.Max(1, voiceCount);
            set
            {
                int normalized = Mathf.Max(1, value);
                if (voiceCount == normalized) return;
                StopAll();
                ReleaseVoices();
                voiceCount = normalized;
            }
        }

        public bool TryPlay(AudioClip clip, float volume = 1f, float pitch = 1f)
        {
            if (clip == null) return false;
            EnsureVoices();
            if (voices.Count == 0) return false;

            AudioSource voice = SelectVoice();
            voice.Stop();
            CopyTemplateSettings(voice);
            voice.clip = clip;
            voice.volume = Mathf.Clamp01(volume);
            voice.pitch = Mathf.Clamp(pitch, -3f, 3f);
            voice.loop = false;
            voice.Play();
            return true;
        }

        public void StopAll()
        {
            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i] != null) voices[i].Stop();
            }
        }

        private AudioSource SelectVoice()
        {
            for (int i = 0; i < voices.Count; i++)
            {
                int index = (nextVoice + i) % voices.Count;
                if (!voices[index].isPlaying)
                {
                    nextVoice = (index + 1) % voices.Count;
                    return voices[index];
                }
            }

            AudioSource selected = voices[nextVoice % voices.Count];
            nextVoice = (nextVoice + 1) % voices.Count;
            return selected;
        }

        private void EnsureVoices()
        {
            if (template == null) template = GetComponent<AudioSource>();
            if (template == null) return;

            while (voices.Count < VoiceCount)
            {
                GameObject voiceObject = new GameObject("Deucarian Audio Voice " + voices.Count)
                {
                    hideFlags = HideFlags.HideInHierarchy
                };
                voiceObject.transform.SetParent(transform, false);
                AudioSource voice = voiceObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voices.Add(voice);
            }
        }

        private void CopyTemplateSettings(AudioSource voice)
        {
            AudioSource source = template != null ? template : GetComponent<AudioSource>();
            voice.outputAudioMixerGroup = source.outputAudioMixerGroup;
            voice.mute = source.mute;
            voice.bypassEffects = source.bypassEffects;
            voice.bypassListenerEffects = source.bypassListenerEffects;
            voice.bypassReverbZones = source.bypassReverbZones;
            voice.priority = source.priority;
            voice.panStereo = source.panStereo;
            voice.spatialBlend = source.spatialBlend;
            voice.reverbZoneMix = source.reverbZoneMix;
            voice.dopplerLevel = source.dopplerLevel;
            voice.spread = source.spread;
            voice.rolloffMode = source.rolloffMode;
            voice.minDistance = source.minDistance;
            voice.maxDistance = source.maxDistance;
        }

        private void ReleaseVoices()
        {
            for (int i = 0; i < voices.Count; i++)
            {
                if (voices[i] != null) UnityObjectUtility.DestroySafely(voices[i].gameObject);
            }

            voices.Clear();
            nextVoice = 0;
        }

        private void OnDestroy() => ReleaseVoices();

        private void Reset()
        {
            template = GetComponent<AudioSource>();
            if (template != null)
            {
                template.playOnAwake = false;
                template.spatialBlend = 0f;
            }
        }

        private void OnValidate() => voiceCount = Mathf.Max(1, voiceCount);
    }
}
