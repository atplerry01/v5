# document

## Purpose

The `document` leaf owns the identity and lifecycle of a stored document — its title, type, classification, and status. It is the primary content artifact of the `document` context.

## Aggregate root

- `DocumentAggregate`

## Key value objects

- `DocumentId`
- `DocumentTitle`
- `DocumentType`
- `DocumentClassification`
- `DocumentStatus` (Draft / Active / Archived)

## Key events

- `DocumentCreatedEvent`
- `DocumentMetadataUpdatedEvent`
- `DocumentActivatedEvent`
- `DocumentArchivedEvent`
- `DocumentRestoredEvent`

## Invariants and lifecycle rules

- A newly created document starts in `Draft`.
- An archived document cannot be modified or re-archived; it can be restored to `Active`.
- `Activate` is valid only from non-archived states; double-activate is rejected.
- `UpdateMetadata` is a no-op when the new title, type, and classification match current values (no event emitted).
- `Restore` is valid only from `Archived`.

## Owns

- Document identity, title, type, classification, status.
- Create / metadata-update / activate / archive / restore transitions.

## References

- No local `*Ref` value objects — the document is a root artifact; other domains reference it via their own `DocumentRef` value objects.

## Does not own

- The stored file bytes — owned by `document/content-artifact/file`.
- Version lineage — owned by `document/lifecycle/version`.
- Metadata entries — owned by `document/descriptor/metadata`.
- Retention — owned by `document/lifecycle/retention`.
- Workflow and approval meaning.

## Notes

- The aggregate is event-sourced; state is rebuilt from its event log via `Apply`.
