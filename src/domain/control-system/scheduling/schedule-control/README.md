# control-system / orchestration / schedule-control

**Classification:** control-system  
**Context:** orchestration  
**Domain:** schedule-control

## Purpose
Scheduling contract for recurring or deferred system job execution. Defines when a system job should be triggered, not how it is executed.

## Scope
- Schedule definition: job definition ID (by reference), trigger expression (cron-like or interval), effective period
- Schedule lifecycle: active → suspended → retired

## Does Not Own
- Job execution (→ runtime)
- Schedule clock ticking (→ runtime / infrastructure)
- Business-specific scheduling rules (deferred)

## Inputs
- job definition ID, trigger expression (string), effective period

## Outputs
- `ScheduleControlDefinedEvent`
- `ScheduleControlSuspendedEvent`
- `ScheduleControlRetiredEvent`

## Invariants
- Schedule ID is deterministic: SHA256 of (jobDefinitionId + triggerExpression)
- Trigger expression must be non-empty
- A retired schedule cannot be reactivated; a new schedule must be created

## Dependencies
- `core-system/identifier` — schedule control ID and job definition reference
- `core-system/temporal` — effective period

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `ScheduleControlAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
