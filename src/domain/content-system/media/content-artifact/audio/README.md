# audio

## Purpose

The `audio` leaf owns the typed audio specialisation of a media asset — its format, duration, and channel count. It is a parallel typed projection alongside the generic `asset`, not a replacement.

## Aggregate root

- `AudioAggregate`

## Key value objects

- `AudioId`
- `MediaAssetRef`
- `MediaFileRef` (optional)
- `AudioFormat`
- `AudioDuration`
- `ChannelCount`
- `AudioStatus` (Draft / Active / Archived)

## Key events

- `AudioCreatedEvent`
- `AudioUpdatedEvent`
- `AudioActivatedEvent`
- `AudioArchivedEvent`

## Invariants and lifecycle rules

- Created in `Draft`.
- Archived is terminal — update, activate, and re-archive are rejected.
- `Update` is a no-op when format, duration, and channels are all unchanged.
- `Activate` rejects already-active or archived.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedAudio`).

## Owns

- Audio-shape truth: format, duration, channel count.
- Create / update / activate / archive transitions.

## References

- `MediaAssetRef` — the generic asset this specialisation belongs to.
- `MediaFileRef` (optional) — the backing file for this audio shape.

## Does not own

- The generic asset — owned by `content-artifact/asset`.
- The backing bytes — owned by `content-artifact/media-file`.
- Audio codec implementation, transcoder logic — infrastructure.
- Subtitle / transcript companions — owned by `companion-artifact/`.
