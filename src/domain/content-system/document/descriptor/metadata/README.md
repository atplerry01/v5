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
