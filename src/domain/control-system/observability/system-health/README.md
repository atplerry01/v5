# system-health

**Classification:** control-system
**Context:** observability
**Domain:** system-health

## Purpose

Represents the evaluated health state of a named system component. A system-health aggregate is a model-driven health record: rather than ad-hoc boolean checks, it tracks degradation and recovery as first-class domain transitions with timestamps and reasons — making health state an auditable, event-sourced fact.

## Aggregate: SystemHealthAggregate

| Property | Type | Description |
|---|---|---|
| Id | SystemHealthId | Deterministic 64-hex SHA256 identifier |
| ComponentName | string | The system component this health record covers |
| Status | HealthStatus | Healthy / Degraded / Unhealthy / Unknown |
| LastEvaluatedAt | DateTimeOffset | Timestamp of the most recent evaluation |

## Invariants

- ComponentName must not be empty.
- Degradation requires a non-empty reason.
- Restore is only valid when the component is not already Healthy.

## Events

| Event | Trigger |
|---|---|
| SystemHealthEvaluatedEvent | Initial health evaluation recorded |
| SystemHealthDegradedEvent | Component health dropped to Degraded or Unhealthy |
| SystemHealthRestoredEvent | Component recovered to Healthy |
