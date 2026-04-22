# system-signal

**Classification:** control-system
**Context:** observability
**Domain:** system-signal

## Purpose

Defines a discrete observable signal emitted by system components. A signal is the canonical declaration of a named, kinded observable — distinct from a metric (which carries a numeric value) and from an alert (which fires on threshold breach). Signals provide the taxonomy of what the system can observe: heartbeats, thresholds, anomalies, recovery confirmations, and degradation notices.

## Aggregate: SystemSignalAggregate

| Property | Type | Description |
|---|---|---|
| Id | SystemSignalId | Deterministic 64-hex SHA256 identifier |
| Name | string | Human-readable signal name |
| Kind | SignalKind | Heartbeat / Threshold / Anomaly / Recovery / Degradation |
| Source | string | Component or subsystem that emits this signal |
| IsDeprecated | bool | Whether the signal has been retired |

## Invariants

- Name and Source must not be empty.
- A deprecated signal cannot be re-deprecated.

## Events

| Event | Trigger |
|---|---|
| SystemSignalDefinedEvent | Signal declared as an observable |
| SystemSignalDeprecatedEvent | Signal retired from active observation |
