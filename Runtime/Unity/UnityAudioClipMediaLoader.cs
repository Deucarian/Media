using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Deucarian.Media.Unity
{
    public sealed class UnityAudioLoadRequest
    {
        public UnityAudioLoadRequest(
            MediaSource source,
            AudioType audioType,
            bool streamAudio = true,
            IReadOnlyDictionary<string, string> headers = null)
        {
            Source = source ??
                     throw new ArgumentNullException(nameof(source));
            AudioType = audioType;
            StreamAudio = streamAudio;
            Headers = headers ??
                      new Dictionary<string, string>();
        }

        public MediaSource Source { get; }
        public AudioType AudioType { get; }
        public bool StreamAudio { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }
    }

    public sealed class UnityAudioClipMediaLoader :
        IMediaLoadStrategy<UnityAudioLoadRequest, AudioClip>
    {
        public bool CanLoad(UnityAudioLoadRequest request)
        {
            return request != null &&
                   request.Source != null &&
                   request.Source.Kind == MediaKind.Audio;
        }

        public async Task<MediaLoadResult<AudioClip>> LoadAsync(
            UnityAudioLoadRequest request,
            CancellationToken cancellationToken)
        {
            if (!CanLoad(request))
            {
                return MediaLoadResult<AudioClip>.Failure(
                    "Audio loader requires an audio media source.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return MediaLoadResult<AudioClip>.Cancelled();
            }

            using (UnityWebRequest webRequest =
                   UnityWebRequestMultimedia.GetAudioClip(
                       request.Source.Location,
                       request.AudioType))
            {
                ApplyHeaders(webRequest, request.Headers);
                DownloadHandlerAudioClip handler =
                    webRequest.downloadHandler as DownloadHandlerAudioClip;
                if (handler != null)
                {
                    handler.streamAudio = request.StreamAudio;
                }

                UnityWebRequestAsyncOperation operation =
                    webRequest.SendWebRequest();
                while (!operation.isDone)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        webRequest.Abort();
                        return MediaLoadResult<AudioClip>.Cancelled();
                    }

                    await Task.Yield();
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return MediaLoadResult<AudioClip>.Cancelled();
                }

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    return MediaLoadResult<AudioClip>.Failure(
                        string.IsNullOrWhiteSpace(webRequest.error)
                            ? "Audio request failed."
                            : webRequest.error);
                }

                AudioClip clip =
                    DownloadHandlerAudioClip.GetContent(webRequest);
                if (clip == null)
                {
                    return MediaLoadResult<AudioClip>.Failure(
                        "Audio response did not contain an AudioClip.");
                }

                return MediaLoadResult<AudioClip>.Success(
                    UnityMediaResourceLease.CreateOwned(clip));
            }
        }

        private static void ApplyHeaders(
            UnityWebRequest request,
            IReadOnlyDictionary<string, string> headers)
        {
            if (request == null || headers == null)
            {
                return;
            }

            foreach (KeyValuePair<string, string> header in headers)
            {
                if (!string.IsNullOrWhiteSpace(header.Key) &&
                    header.Value != null)
                {
                    request.SetRequestHeader(
                        header.Key,
                        header.Value);
                }
            }
        }
    }
}

