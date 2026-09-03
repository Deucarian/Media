# Deucarian Media

Reusable typed media loading and playback primitives for Unity projects.

## Short UI audio output

`UnityAudioOneShotOutput` is the package-owned Unity adapter for short,
overlapping feedback clips. It keeps concrete `AudioSource` playback in Media
while semantic role and palette ownership remains in Theming. The output uses
a small voice pool so each request can keep its own pitch. Missing clips are a
safe no-op.

Current package version: `0.1.0`

## Responsibilities

- Typed sources for image, audio, video, text, and binary media.
- Generic loader and strategy contracts.
- Cancellation-aware request coordination with stale-result disposal.
- Explicit, idempotent resource leases.
- Playback session contracts and immutable snapshots.
- Unity audio loading and `VideoPlayer` playback adapters.

Application attachment models, authentication policy, 3D placement, and
product UI remain in their owning applications.

## Installation

Stable:

```json
"com.deucarian.media": "https://github.com/Deucarian/Media.git#main"
```

Development:

```json
"com.deucarian.media": "https://github.com/Deucarian/Media.git#develop"
```

## Architecture

`Deucarian.Media` is engine-independent. `Deucarian.Media.Unity` owns Unity
networking, audio, video, and Unity object-lifetime side effects.

The canonical architecture standard is maintained by the
[Deucarian Package Registry](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md).

