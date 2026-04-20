# media / core-object

## Purpose

Groups the **primary durable objects** of the media context — `asset` (absorbing former audio/video/image intrinsic descriptors), and the companions `subtitle` and `transcript`.

## Leaf domains

- `asset/` — the generic media asset aggregate (identity, title, classification, status) plus the intrinsic type-specific VOs absorbed in CS.8 per §CD-03. Pending CS.13: `AssetKind` discriminator + kind-qualified intrinsic VO wiring.
- `subtitle/` — subtitle companion artifact (language, format, window, status).
- `transcript/` — transcript companion artifact (language, format, status).
- `rendition/` — (SCAFFOLD pending CS.10) derived encoded variants of an asset (bitrate/codec/container).
- `artwork/` — (SCAFFOLD pending CS.10) associated artwork (thumbnail, cover).
- `preview/` — (SCAFFOLD pending CS.10) preview rendering of an asset.

## Boundary notes

- Asset is the root truth of the media context. Subtitle and transcript are COMPANIONS — they reference assets via opaque refs but do not own asset identity.
- The retired `media-file` aggregate's storage concerns are now infrastructure. Per §CD-04, `MediaFileRef` VOs (held locally by subtitle/transcript/version) are opaque references to infrastructure-owned storage; the aggregate is gone.
- Intrinsic type-specific VOs (AudioDuration, VideoDimensions, ImageOrientation, etc.) live on `asset/` per §CD-03.
- Evaluative/measured VOs (bitrate, quality scores, loudness) belong in `technical-processing/quality/`.
