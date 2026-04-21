# metadata

## Purpose

The `metadata` leaf owns descriptive metadata attached to a media asset. It is an open-then-finalized aggregate of typed key/value entries, and it also hosts canonical media-descriptor value objects (Bitrate, CodecName, Dimensions, Duration, LanguageTag).

## Aggregate root

- `MediaMetadataAggregate`

## Key value objects

- `MediaMetadataId`
- `MediaAssetRef`
- `MediaMetadataKey`
- `MediaMetadataValue`
- `MediaMetadataEntry`
- `MediaMetadataStatus` (Open / Finalized)
- Canonical media-descriptor types: `Bitrate`, `CodecName`, `Dimensions`, `Duration`, `LanguageTag`

## Key events

- `MediaMetadataCreatedEvent`
- `MediaMetadataEntryAddedEvent`
- `MediaMetadataEntryUpdatedEvent`
- `MediaMetadataEntryRemovedEvent`
- `MediaMetadataFinalizedEvent`

## Invariants and lifecycle rules

- Entries are a map keyed by `MediaMetadataKey`; duplicate-add is rejected (`DuplicateKey`), unknown-update/remove is rejected (`UnknownKey`).
- `UpdateEntry` is a no-op if the new value equals the existing value.
- `Finalize` requires at least one entry (`EmptyMetadata`).
- After `Finalize`, all mutations are rejected.
- `AssetRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedMetadata`).

## Owns

- Entry set, status, finalized-at timestamp.
- Create / add / update / remove / finalize transitions.
- Canonical media-descriptor value-object shapes shared across the `media` context.

## References

- `MediaAssetRef` — back-pointer to the described asset.

## Does not own

- The media asset itself.
- Catalogue surfaces or search — downstream.
- Descriptor extraction / probing (e.g. via ffprobe) — infrastructure driven from `media/lifecycle/processing`.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanModifyMediaMetadataSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `MediaMetadataAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
