using System.Collections.Generic;
using Deucarian.Media.Unity;
using NUnit.Framework;
using UnityEngine;

namespace Deucarian.Media.Tests
{
    public sealed class UnityAudioOneShotOutputTests
    {
        private readonly List<Object> created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = 0; i < created.Count; i++)
            {
                if (created[i] != null) Object.DestroyImmediate(created[i]);
            }
            created.Clear();
        }

        [Test]
        public void MissingClipIsSafeNoOp()
        {
            GameObject host = new GameObject("Audio Output");
            created.Add(host);
            host.AddComponent<AudioSource>();
            UnityAudioOneShotOutput output = host.AddComponent<UnityAudioOneShotOutput>();

            Assert.IsFalse(output.TryPlay(null));
            Assert.DoesNotThrow(output.StopAll);
        }

        [Test]
        public void VoiceCountIsAlwaysPositive()
        {
            GameObject host = new GameObject("Audio Output");
            created.Add(host);
            host.AddComponent<AudioSource>();
            UnityAudioOneShotOutput output = host.AddComponent<UnityAudioOneShotOutput>();

            output.VoiceCount = 0;
            Assert.AreEqual(1, output.VoiceCount);
        }
    }
}
