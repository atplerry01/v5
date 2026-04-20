# streaming / delivery-artifact

## Purpose

Groups artifacts produced in order to deliver a stream to a consumer: the manifest, its segments, and the playback descriptor that governs playback availability.

## Why this group exists

Manifests, segments, and playback descriptors are all **produced outputs** of streaming — each one exists to make a stream consumable by a player. They share the same semantic class (delivery-side technical artifacts) and sit downstream of `stream-core/`. Isolating them into `delivery-artifact/` keeps that distinction clear.

## Leaf domains

- `manifest/` — streaming manifest (e.g. HLS/DASH-shaped) with version progression. Created / Published / Retired / Archived, with `Update()` incrementing `ManifestVersion`.
- `segment/` — individual delivery segment (SourceRef, SequenceNumber, Window). Created / Published / Retired / Archived.
- `playback/` — playback descriptor governing technical playback availability. Created / Enabled / Disabled / Archived, with window updates.

## Boundary notes

- Each delivery artifact carries its own `*SourceRef` value object (`ManifestSourceRef`, `SegmentSourceRef`, `PlaybackSourceRef`), not a generic stream pointer. This keeps delivery artifacts decoupled from specific stream identity at the type level.
- Delivery-side adapters (packagers, CDN configuration) are infrastructure and are **not** modelled here — these aggregates only describe the delivery artifact's existence and lifecycle.
- DRM key management and licence issuance are policy/infrastructure concerns and are not in scope.
