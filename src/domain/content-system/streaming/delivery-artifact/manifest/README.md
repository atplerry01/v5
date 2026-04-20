# manifest

## Purpose

The `manifest` leaf owns streaming manifest artifacts — identity, source, version progression, and published/retired/archived lifecycle. The manifest is the primary delivery descriptor a player consumes to discover segments.

## Aggregate root

- `ManifestAggregate`

## Key value objects

- `ManifestId`
- `ManifestSourceRef`
- `ManifestSourceKind`
- `ManifestVersion`
- `ManifestStatus` (Created / Published / Retired / Archived)

## Key events

- `ManifestCreatedEvent`
- `ManifestPublishedEvent`
- `ManifestUpdatedEvent`
- `ManifestRetiredEvent`
- `ManifestArchivedEvent`

## Invariants and lifecycle rules

- Created with `ManifestVersion(1)` in status `Created`.
- `Publish` rejects on archived, retired, or already-published.
- `Update` is valid only when currently `Published`; it increments `ManifestVersion` via `CurrentVersion.Next()` and emits `ManifestUpdatedEvent`.
- `Retire` requires a non-empty reason; rejects archived or already-retired.
- `Archive` is terminal.
- `SourceRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedManifest`).

## Owns

- Manifest identity, source reference and kind, current version, status, published/created timestamps.
- Publish / update / retire / archive transitions.

## References

- `ManifestSourceRef` — opaque source the manifest describes.
- `ManifestSourceKind` — typed discriminator of the source.

## Does not own

- The source stream — owned by `stream-core/stream`.
- Segments — owned by `delivery-artifact/segment`.
- Playback availability — owned by `delivery-artifact/playback`.
- The specific HLS/DASH/CMAF representation — that is a projection / infrastructure concern.
