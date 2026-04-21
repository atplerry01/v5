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

- The document — owned by `document/core-object/document`.
- Any legal semantics of a "record of" the document — purely a technical container.
- Access or permission to the record — belongs to policy/identity layers.

## Notes

- Lock/Unlock is a reversible pause; Close is a terminal transition with a reason.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanCloseRecordSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `DocumentRecordAggregate.Create(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
