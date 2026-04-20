# record

## Purpose

The `record` leaf owns a lockable/closable container around a document — used to represent a record of the document that carries its own open/locked/closed/archived lifecycle independent of the document itself.

## Aggregate root

- `DocumentRecordAggregate`

## Key value objects

- `DocumentRecordId`
- `DocumentRef`
- `RecordStatus` (Open / Locked / Closed / Archived)
- `RecordClosureReason`

## Key events

- `DocumentRecordCreatedEvent`
- `DocumentRecordLockedEvent`
- `DocumentRecordUnlockedEvent`
- `DocumentRecordClosedEvent`
- `DocumentRecordArchivedEvent`

## Invariants and lifecycle rules

- `Lock` requires a non-empty reason and is valid only from `Open`.
- `Unlock` is valid only from `Locked`.
- An archived or closed record cannot be locked, unlocked, or closed again (terminal gates).
- `Close` captures a `RecordClosureReason`; double-close is rejected.
- `DocumentRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedRecord`).

## Owns

- Record identity, status, closure reason, created/closed timestamps.
- Create / lock / unlock / close / archive transitions.

## References

- `DocumentRef` — back-pointer to the referenced document.

## Does not own

- The document — owned by `document/content-artifact/document`.
- Any legal semantics of a "record of" the document — purely a technical container.
- Access or permission to the record — belongs to policy/identity layers.

## Notes

- Lock/Unlock is a reversible pause; Close is a terminal transition with a reason.
