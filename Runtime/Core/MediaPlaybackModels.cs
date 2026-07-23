using System;
using System.Collections.Generic;

namespace Deucarian.Media
{
    public enum MediaPlaybackState
    {
        Idle,
        Preparing,
        Ready,
        Playing,
        Paused,
        Stopped,
        Failed,
        Disposed
    }

    public readonly struct MediaPlaybackSnapshot :
        IEquatable<MediaPlaybackSnapshot>
    {
        public MediaPlaybackSnapshot(
            MediaPlaybackState state,
            double time,
            double duration)
        {
            State = state;
            Time = Math.Max(0d, time);
            Duration = Math.Max(0d, duration);
        }

        public MediaPlaybackState State { get; }
        public double Time { get; }
        public double Duration { get; }
        public bool IsPrepared =>
            State == MediaPlaybackState.Ready ||
            State == MediaPlaybackState.Playing ||
            State == MediaPlaybackState.Paused ||
            State == MediaPlaybackState.Stopped;
        public bool IsPlaying => State == MediaPlaybackState.Playing;

        public bool Equals(MediaPlaybackSnapshot other)
        {
            return State == other.State &&
                   Time.Equals(other.Time) &&
                   Duration.Equals(other.Duration);
        }

        public override bool Equals(object obj)
        {
            return obj is MediaPlaybackSnapshot other &&
                   Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = (int)State;
                hash = (hash * 397) ^ Time.GetHashCode();
                hash = (hash * 397) ^ Duration.GetHashCode();
                return hash;
            }
        }
    }

    public sealed class MediaPlaybackPrepareRequest<TOutput>
    {
        public MediaPlaybackPrepareRequest(
            MediaSource source,
            TOutput output,
            IReadOnlyDictionary<string, string> headers = null)
        {
            Source = source ??
                     throw new ArgumentNullException(nameof(source));
            Output = output;
            Headers = headers ??
                      new Dictionary<string, string>();
        }

        public MediaSource Source { get; }
        public TOutput Output { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
    }

    public sealed class MediaPlaybackPrepareResult
    {
        private MediaPlaybackPrepareResult(
            bool succeeded,
            bool cancelled,
            int width,
            int height,
            double duration,
            string error)
        {
            Succeeded = succeeded;
            Cancelled = cancelled;
            Width = Math.Max(0, width);
            Height = Math.Max(0, height);
            Duration = Math.Max(0d, duration);
            Error = error;
        }

        public bool Succeeded { get; }
        public bool Cancelled { get; }
        public int Width { get; }
        public int Height { get; }
        public double Duration { get; }
        public string Error { get; }

        public static MediaPlaybackPrepareResult Success(
            int width,
            int height,
            double duration)
        {
            return new MediaPlaybackPrepareResult(
                true,
                false,
                width,
                height,
                duration,
                null);
        }

        public static MediaPlaybackPrepareResult Failure(string error)
        {
            return new MediaPlaybackPrepareResult(
                false,
                false,
                0,
                0,
                0d,
                string.IsNullOrWhiteSpace(error)
                    ? "Media playback preparation failed."
                    : error.Trim());
        }

        public static MediaPlaybackPrepareResult CancelledResult()
        {
            return new MediaPlaybackPrepareResult(
                false,
                true,
                0,
                0,
                0d,
                null);
        }
    }

    public sealed class MediaPlaybackSnapshotChangedEventArgs :
        EventArgs
    {
        public MediaPlaybackSnapshotChangedEventArgs(
            MediaPlaybackSnapshot previous,
            MediaPlaybackSnapshot current)
        {
            Previous = previous;
            Current = current;
        }

        public MediaPlaybackSnapshot Previous { get; }
        public MediaPlaybackSnapshot Current { get; }
    }
}

