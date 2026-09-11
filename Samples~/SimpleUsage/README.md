# Simple usage

Add a MediaPlayerHost and select audio or video in the Inspector. For audio, set the AudioType appropriate to your files. For video, assign a RenderTexture that your own UI displays, or call ConfigureVideo(output) once. The host owns the load result and playback session, releases the previous audio when replaced/stopped, and cancels pending preparation on disable. A late load cannot restart playback after Stop. PlayAsync returns MediaPlaybackPrepareResult; the caller does not dispose resource leases. An explicitly supplied RenderTexture remains caller-owned.

Import the **Simple Usage** sample from Unity Package Manager. Its caller script is:

```csharp
using UnityEngine;

namespace Deucarian.Media.Unity.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private MediaPlayerHost player;
        public System.Threading.Tasks.Task Play(string url) => player.PlayAsync(url);
        public void Stop() => player.Stop();
    }
}
```
