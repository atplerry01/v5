# control-system / observability / system-alert

**Classification:** control-system  
**Context:** observability  
**Domain:** system-alert

## Purpose
Alert definition expressing a condition against a declared system metric: when the condition is met, the alert fires. This domain owns the structural definition only — not delivery, not storage.

## Scope
- Alert definition: name, metric reference, condition expression (threshold, comparator), severity
- Alert lifecycle: defined → active → suppressed → retired (status value — no state machine)

## Does Not Own
- Alert delivery or notification (→ infrastructure)
- Metric collection (→ system-metric, runtime)
- Business-specific alerting (deferred — domain semantics)

## Inputs
- name, metric definition ID (reference by ID only), condition expression, severity (warning|critical)

## Outputs
- `SystemAlertDefinedEvent`
- `SystemAlertFiredEvent`
- `SystemAlertRetiredEvent`

## Invariants
- Alert ID is deterministic: SHA256 of (name + metricId + condition)
- Every alert references exactly one declared system-metric
- Alert names are globally unique within the control-system
- Published alert definitions are immutable

## Dependencies
- `core-system/identifier` — alert definition ID
- `core-system/temporal` — alert effective window

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `SystemAlertAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
