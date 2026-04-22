# control-system / observability / system-metric

**Classification:** control-system  
**Context:** observability  
**Domain:** system-metric

## Purpose
Structural definition of a system metric: its name, type (counter / gauge / histogram), unit of measurement, and label schema. Not the metric value — only the structural contract for what can be measured.

## Scope
- Metric definition registration: name, type, unit, label declarations
- Label schema: label names and allowed value constraints

## Does Not Own
- Metric value collection (→ runtime / infrastructure)
- Alerting rules (→ system-alert)

## Inputs
- name (string, follows `{classification}_{context}_{name}`), type (counter|gauge|histogram), unit, label schema

## Outputs
- `SystemMetricDefinedEvent`
- `SystemMetricDeprecatedEvent`

## Invariants
- Metric ID is deterministic: SHA256 of (name + type)
- Names are globally unique and follow the naming convention
- Label schemas are immutable once registered
- Published definitions are immutable

## Dependencies
- `core-system/identifier` — metric definition ID

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `SystemMetricAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
