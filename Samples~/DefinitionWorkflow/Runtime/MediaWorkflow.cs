using System;
using UnityEngine;

namespace Deucarian.Media.Unity.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class MediaWorkflow : MonoBehaviour
    {
        [SerializeField] private MediaPlayerHost host;
        [SerializeField] private MediaKey media;
        [SerializeField] private MediaTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public async void Play() { var result = await host.PlayAsync(media); status = result.Succeeded ? "Playing the local sample clip." : result.Error; }
        public void PlayComponent() { trigger.Play(); status = "Playing through MediaTrigger."; }
        public void Stop() { host.Stop(); status = "Stopped. The host released playback."; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("Media — definition workflow");
            GUILayout.Label("One typed media definition holds the clip or URL and audio playback defaults. This scene uses a bundled local clip.");
            GUILayout.Space(12);
            if (GUILayout.Button("Play with C#", GUILayout.Height(32))) { try { Play(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Play with component", GUILayout.Height(32))) { try { PlayComponent(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Stop", GUILayout.Height(32))) { try { Stop(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}
