# Simple usage

Add a MediaPlayerHost and select audio or video in the Inspector. For audio, set the AudioType appropriate to your files. For video, assign a RenderTexture that your own UI displays, or call ConfigureVideo(output) once. The host owns the load result and playback session, releases the previous audio when replaced/stopped, and cancels pending preparation on disable. A late load cannot restart playback after Stop. PlayAsync returns MediaPlaybackPrepareResult; the caller does not dispose resource leases. An explicitly supplied RenderTexture remains caller-owned.

Import the **Simple Usage** sample from Unity Package Manager. Its caller script is:

Definition fields now use named, domain-specific keys. Select an existing definition from the Inspector dropdown or pass the same named key in code. Declare each project key once in a marked key set; ordinary caller methods do not accept raw IDs. Generated keys for asset-authored definitions require no asset reference in the caller. Owner-issued selection and row handles represent runtime instances.

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
