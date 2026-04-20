# video

## Purpose

The `video` leaf owns the typed video specialisation of a media asset — its dimensions, duration, and frame rate. It is a parallel typed projection alongside the generic `asset`, not a replacement.

## Aggregate root

- `VideoAggregate`

## Key value objects

- `VideoId`
- `MediaAssetRef`
- `MediaFileRef` (optional)
- `VideoDimensions`
- `VideoDuration`
- `FrameRate`
- `VideoStatus` (Draft / Active / Archived)

## Key events

- `VideoCreatedEvent`
- `VideoUpdatedEvent`
- `VideoActivatedEvent`
- `VideoArchivedEvent`

## Invariants and lifecycle rules

- Created in `Draft`.
- Archived is terminal — update, activate, and re-archive are rejected.
- `Update` is a no-op when dimensions, duration, and frame rate are all unchanged.
- `Activate` rejects already-active or archived.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedVideo`).

## Owns

- Video-shape truth: dimensions, duration, frame rate.
- Create / update / activate / archive transitions.

## References

- `MediaAssetRef` — the generic asset this specialisation belongs to.
- `MediaFileRef` (optional) — the backing file for this video shape.

## Does not own

- The generic asset — owned by `content-artifact/asset`.
- The backing bytes — owned by `content-artifact/media-file`.
- Transcoding, packaging, DRM — infrastructure / streaming delivery.
- Subtitle / transcript companions — owned by `companion-artifact/`.
