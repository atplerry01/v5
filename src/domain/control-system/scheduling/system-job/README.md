# control-system / orchestration / system-job

**Classification:** control-system  
**Context:** orchestration  
**Domain:** system-job

## Purpose
Definition of a system-level administrative job: its name, category, idempotency contract, and execution parameters. A job definition is the template; execution instances are tracked separately via execution-control.

## Scope
- Job definition: name, category (maintenance|reconciliation|audit|diagnostic), idempotency key strategy, timeout
- Job definition versioning and deprecation

## Does Not Own
- Job execution (→ runtime)
- Job scheduling (→ schedule-control)
- Execution instance tracking (→ execution-control)

## Inputs
- name, category, idempotency strategy, timeout window

## Outputs
- `SystemJobDefinedEvent`
- `SystemJobDeprecatedEvent`

## Invariants
- Job definition ID is deterministic: SHA256 of (name + version)
- Category must be one of the four declared values
- Published job definitions are immutable

## Dependencies
- `core-system/identifier` — job definition ID
- `core-system/temporal` — timeout window

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `SystemJobAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
