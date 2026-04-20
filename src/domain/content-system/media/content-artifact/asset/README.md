# asset

## Purpose

The `asset` leaf owns the generic media asset — its identity, title, classification, and draft/active/retired lifecycle. It is the primary content artifact of the `media` context and the anchor that typed specialisations (audio, video, image) and companions (subtitle, transcript) refer to.

## Aggregate root

- `AssetAggregate`

## Key value objects

- `AssetId`
- `AssetTitle`
- `AssetClassification`
- `AssetStatus` (Draft / Active / Retired)

## Key events

- `AssetCreatedEvent`
- `AssetRenamedEvent`
- `AssetReclassifiedEvent`
- `AssetActivatedEvent`
- `AssetRetiredEvent`

## Invariants and lifecycle rules

- A newly created asset starts in `Draft`.
- A retired asset is immutable — rename, reclassify, activate, and re-retire are rejected.
- `Rename` and `Reclassify` are no-ops when the new value matches current.
- `Activate` rejects already-active or retired.
- `EnsureInvariants` requires that an active asset is **not** classified as `AssetClassification.Unclassified` (`MissingClassification`).

## Owns

- Asset identity, title, classification, status.
- Create / rename / reclassify / activate / retire transitions.

## References

- No local `*Ref` — the asset is the root artifact of the media context. Typed specialisations, companions, metadata, and lifecycle aggregates reference it via `MediaAssetRef`.

## Does not own

- Typed shape (audio format, video dimensions, image orientation) — owned by `audio`, `video`, `image` specialisation leaves.
- Backing file bytes — owned by `media-file`.
- Metadata entries — owned by `descriptor/metadata`.
- Upload, processing, version lifecycle — owned by the `lifecycle/` group.
