# control-system / audit / audit-log

**Classification:** control-system  
**Context:** audit  
**Domain:** audit-log

## Purpose
Immutable append-only log of system-significant events. Each entry captures: who acted, what action was taken, what resource was affected, what decision governed it, and when.

## Scope
- Audit entry creation from system event receipt
- Entry classification: access / command / policy / state
- Correlation to upstream event and decision IDs

## Does Not Own
- Analysis or finding logic (→ audit-finding)
- Enforcement records (→ system-policy/policy-enforcement)

## Inputs
- System event receipt (actor, action, resource, decision ID, timestamp)

## Outputs
- `AuditEntryRecordedEvent`

## Invariants
- Entries are immutable once written
- Entry ID is deterministic: SHA256 of (actor + action + resource + decisionId + timestamp)
- Classification must be declared at creation

## Dependencies
- `core-system/identifier` — deterministic entry ID
- `core-system/temporal` — entry timestamp

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `AuditLogAggregate.Record(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
