# control-system / orchestration / execution-control

**Classification:** control-system  
**Context:** orchestration  
**Domain:** execution-control

## Purpose
Control record for a system job instance: the structural representation of start, stop, and suspend signals issued against a running job. Not the execution itself — only the control intent.

## Scope
- Execution control record: jobInstanceId, signal (start|stop|suspend|resume), issuedAt, issuedBy (actor reference)
- Control outcome: acknowledged / rejected / applied

## Does Not Own
- Job execution (→ runtime / engine)
- Job definition (→ system-job)
- Policy authorization of the signal (→ system-policy)

## Inputs
- jobInstanceId, signal type, actor reference (opaque ID)

## Outputs
- `ExecutionControlSignalIssuedEvent`
- `ExecutionControlSignalAppliedEvent`
- `ExecutionControlSignalRejectedEvent`

## Invariants
- Control record ID is deterministic: SHA256 of (jobInstanceId + signal + issuedAt)
- Signal type is one of: start / stop / suspend / resume
- Records are immutable once created

## Dependencies
- `core-system/identifier` — control record and job instance IDs
- `core-system/temporal` — issuedAt timestamp

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `ExecutionControlAggregate.Issue(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
