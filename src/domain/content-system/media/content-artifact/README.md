# media / content-artifact

## Purpose

Groups the primary media content artifacts: the generic asset, its typed specialisations (audio, video, image), and the underlying media-file bytes that back them.

## Why this group exists

A media asset is the primary content object of the `media` context. Its typed specialisations (audio/video/image) are specialisations of that same "primary artifact" class — they describe the typed media shape while the asset owns the generic identity. The media-file sits alongside them as the stored byte truth. All five share the same semantic class (primary content artifacts) and belong together.

Separating them from `companion-artifact/` (subtitle, transcript — accompaniments, not primary artifacts), from `descriptor/` (metadata), and from `lifecycle/` (upload/processing/version transactions) keeps the semantic classes distinct.

## Leaf domains

- `asset/` — the generic media asset aggregate (identity, title, classification, status).
- `audio/` — typed audio specialisation (format, duration, channel count).
- `video/` — typed video specialisation (dimensions, duration, frame rate).
- `image/` — typed image specialisation (dimensions, orientation).
- `media-file/` — stored media-file bytes with integrity and supersession.

## Boundary notes

- Typed specialisations (`audio`, `video`, `image`) reference the generic `asset` via `MediaAssetRef` and the backing file via `MediaFileRef`. They do not replace the asset — they are parallel typed projections.
- Subtitle and transcript are **companions**, not primary artifacts, and live in `companion-artifact/`.
- Metadata is a descriptor class and lives in `descriptor/metadata`.
- Upload, processing, and version lifecycle aggregates live in `lifecycle/`.
