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
