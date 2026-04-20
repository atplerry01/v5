# image

## Purpose

The `image` leaf owns the typed image specialisation of a media asset — its dimensions and orientation. It is a parallel typed projection alongside the generic `asset`, not a replacement.

## Aggregate root

- `ImageAggregate`

## Key value objects

- `ImageId`
- `MediaAssetRef`
- `MediaFileRef` (optional)
- `ImageDimensions`
- `ImageOrientation`
- `ImageStatus` (Draft / Active / Archived)

## Key events

- `ImageCreatedEvent`
- `ImageUpdatedEvent`
- `ImageActivatedEvent`
- `ImageArchivedEvent`

## Invariants and lifecycle rules

- Created in `Draft`.
- Archived is terminal — update, activate, and re-archive are rejected.
- `Update` is a no-op when dimensions and orientation are unchanged.
- `Activate` rejects already-active or archived.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedImage`).

## Owns

- Image-shape truth: dimensions, orientation.
- Create / update / activate / archive transitions.

## References

- `MediaAssetRef` — the generic asset this specialisation belongs to.
- `MediaFileRef` (optional) — the backing file for this image shape.

## Does not own

- The generic asset — owned by `content-artifact/asset`.
- The backing bytes — owned by `content-artifact/media-file`.
- Image processing, thumbnail generation, colour-profile transformation — infrastructure.
- EXIF / descriptor metadata — owned by `descriptor/metadata`.
