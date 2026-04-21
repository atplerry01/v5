# metadata

## Purpose

The `metadata` leaf owns descriptive metadata attached to a document. It is an open-then-finalized aggregate of typed key/value entries.

## Aggregate root

- `DocumentMetadataAggregate`

## Key value objects

- `DocumentMetadataId`
- `DocumentRef`
- `MetadataKey`
- `MetadataValue`
- `MetadataEntry`
- `MetadataStatus` (Open / Finalized)

## Key events

- `DocumentMetadataCreatedEvent`
- `DocumentMetadataEntryAddedEvent`
- `DocumentMetadataEntryUpdatedEvent`
- `DocumentMetadataEntryRemovedEvent`
- `DocumentMetadataFinalizedEvent`

## Invariants and lifecycle rules

- Entries are a map keyed by `MetadataKey`; duplicate-add is rejected (`DuplicateKey`), unknown-update/remove is rejected (`UnknownKey`).
- `UpdateEntry` is a no-op if the new value equals the existing value.
- `Finalize` requires at least one entry (`EmptyMetadata`).
- After `Finalize`, all mutations are rejected.
- `DocumentRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedMetadata`).

## Owns

- Entry set, status, finalized-at timestamp.
- Create / add / update / remove / finalize transitions.

## References

- `DocumentRef` — back-pointer to the described document.

## Does not own

- The document itself.
- Search indexing, discovery surfaces, facets — those are downstream.
- Schema validation of `MetadataKey`/`MetadataValue` beyond what the value objects themselves enforce.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (`MetadataEntry` is modelled as a VO inside `value-object/`, not a child entity with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanModifyMetadataSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `DocumentMetadataAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
