# Deucarian Media

## Typed definition workflow

One typed media definition holds the clip or URL and audio playback defaults. This scene uses a bundled local clip.

Start with the [Definition Workflow walkthrough](Documentation~/DefinitionWorkflow.md).
Import **Definition Workflow** in Package Manager for a configured sample scene
and short caller scripts. Definitions can be edited as assets or editable C# declarations; generated keys
work in code and Inspector dropdowns.


For simple calls and setup, see [Simple usage](Documentation~/SimpleUsage.md).

Reusable typed media loading and playback primitives for Unity projects.

## Short UI audio output

`UnityAudioOneShotOutput` is the package-owned Unity adapter for short,
overlapping feedback clips. It keeps concrete `AudioSource` playback in Media
while semantic role and palette ownership remains in Theming. The output uses
a small voice pool so each request can keep its own pitch. Missing clips are a
safe no-op.

Current package version: `0.3.0`

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

