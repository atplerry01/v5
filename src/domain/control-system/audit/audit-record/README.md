# control-system / audit / audit-record

**Classification:** control-system  
**Context:** audit  
**Domain:** audit-record

## Purpose
A structured audit record raised against one or more audit log entries. Captures a detected condition, its severity, and any recommended remediation. Records are append-only and immutable once created.

## Scope
- Audit record creation referencing one or more audit log entry IDs
- Severity classification: informational / warning / violation / critical
- Record lifecycle: open → acknowledged → resolved → closed (status field only — no state machine)

## Does Not Own
- Audit log entry creation (→ audit-log)
- Policy remediation (→ system-policy)
- Compliance determinations (deferred — domain semantics)

## Inputs
- One or more audit log entry IDs, description, severity, recommended action

## Outputs
- `AuditRecordRaisedEvent`
- `AuditRecordResolvedEvent`

## Invariants
- Every record references at least one audit log entry
- Severity must be one of the four declared levels
- Records are immutable after creation; resolution is tracked as a status value, not a delete

## Dependencies
- `core-system/identifier` — record ID
- `core-system/temporal` — raised-at and resolved-at timestamps

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `AuditRecordAggregate.Raise(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
