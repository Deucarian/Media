namespace Deucarian.Media.Samples
{
    public static class TypedMediaSourceExample
    {
        public static MediaSource CreateImage(string url)
        {
            return new MediaSource(
                url,
                MediaKind.Image,
                contentType: "image/*");
        }

        public static MediaSource CreateAudio(string url)
        {
            return new MediaSource(
                url,
                MediaKind.Audio,
                contentType: "audio/*");
        }

        public static MediaSource CreateVideo(string url)
        {
            return new MediaSource(
                url,
                MediaKind.Video,
                contentType: "video/*");
        }

        public static MediaSource CreateText(string url)
        {
            return new MediaSource(
                url,
                MediaKind.Text,
                contentType: "text/plain");
        }
    }
}

