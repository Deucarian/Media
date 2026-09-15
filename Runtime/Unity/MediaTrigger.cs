using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
namespace Deucarian.Media.Unity
{
    public sealed class MediaTrigger : MonoBehaviour
    {
        [SerializeField] private MediaPlayerHost host;
        [SerializeField] private MediaKey media;
        [SerializeField] private UnityEvent playing = new UnityEvent();
        [SerializeField] private UnityEvent failed = new UnityEvent();
        public MediaPlaybackPrepareResult LastResult { get; private set; }
        private MediaPlayerHost Host => host != null ? host : throw new InvalidOperationException("Assign a MediaPlayerHost to this MediaTrigger.");
        public Task<MediaPlaybackPrepareResult> PlayAsync() => Host.PlayAsync(media);
        public async void Play()
        {
            LastResult = await PlayAsync();
            if (this == null) return;
            if (LastResult.Succeeded) playing.Invoke(); else failed.Invoke();
        }
        public void Stop() => Host.Stop();
    }
}
