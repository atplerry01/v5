# segment

## Purpose

The `segment` leaf owns individual delivery segments — identity, source, sequence number, time window, and a created/published/retired/archived lifecycle.

## Aggregate root

- `SegmentAggregate`

## Key value objects

- `SegmentId`
- `SegmentSourceRef`
- `SegmentSourceKind`
- `SegmentSequenceNumber`
- `SegmentWindow`
- `SegmentStatus` (Created / Published / Retired / Archived)

## Key events

- `SegmentCreatedEvent`
- `SegmentPublishedEvent`
- `SegmentRetiredEvent`
- `SegmentArchivedEvent`

## Invariants and lifecycle rules

- Created in `Created`.
- `Publish` rejects archived, retired, or already-published.
- `Retire` rejects archived or already-retired.
- `Archive` is terminal.
- `SourceRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedSegment`).

## Owns

- Segment identity, source reference and kind, sequence number, time window, status, published-at timestamp.
- Create / publish / retire / archive transitions.

## References

- `SegmentSourceRef` — opaque source the segment was produced from.
- `SegmentSourceKind` — typed discriminator.

## Does not own

- The containing manifest — owned by `delivery-artifact/manifest`.
- The source stream — owned by `stream-core/stream`.
- Encryption keys / DRM — infrastructure / policy.
- The CDN where the segment is served — infrastructure.
