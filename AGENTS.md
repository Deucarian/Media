# Deucarian Media Agent Notes

Package ID: `com.deucarian.media`
Repository: `Deucarian/Media`

Follow the canonical [Deucarian Architecture Rules](https://github.com/Deucarian/Package-Registry/blob/main/ARCHITECTURE.md).

## Ownership

This package owns typed media loading contracts, load strategies, request
versioning, resource leases, playback contracts, and Unity audio/video
adapters.

It must not own application attachment metadata, API authentication policy,
3D report pose rules, viewer overlays, or product-specific playback controls.

## Dependencies

- `com.deucarian.common`: canonical Unity object lifetime handling.
- `com.deucarian.editor`: editor-only media definition schemas, typed dropdowns
  and generation. Core/player assemblies never reference Editor.
- Unity Audio, Video, UnityWebRequest, and UnityWebRequest Audio modules:
  platform adapters directly use these Unity APIs.

Core contracts live in `Deucarian.Media` with no engine references. Unity
adapters live in `Deucarian.Media.Unity`.

## Validation

```powershell
python C:/Repositories/Package-Registry/Tools/deucarian_package_validator.py --registry-root C:/Repositories/Package-Registry --repository-root . --config deucarian-package.json
```

Work on `develop`. Promote to `main` only as a deliberate stable-channel
operation. Do not edit `Library/PackageCache`.

