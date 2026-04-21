# metrics

## Purpose

The `metrics` leaf owns stream-native technical metrics — captured, updated, and finalized measurements of a stream's runtime behaviour (bitrate, latency, drops, errors, viewer and playback counts).

## Aggregate root

- `MetricsAggregate`

## Key value objects

- `MetricsId`
- `StreamRef`
- `RecordingRef` (optional)
- `MetricsWindow`
- `MetricsSnapshot`
- `MetricsStatus` (Capturing / Updated / Finalized / Archived)
- Measurement value objects: `BitrateMeasurement`, `LatencyMeasurement`, `DropCount`, `ErrorCount`, `ViewerCount`, `PlaybackCount`

## Key events

- `MetricsCapturedEvent`
- `MetricsUpdatedEvent`
- `MetricsFinalizedEvent`
- `MetricsArchivedEvent`

## Invariants and lifecycle rules

- `Capture` creates the metrics in `Capturing` with an initial `MetricsSnapshot`.
- `Update` rejects if finalized or archived; it is a no-op when the new snapshot equals the current one, otherwise it emits `MetricsUpdatedEvent` and moves status to `Updated`.
- `Finalize` rejects already-finalized or archived.
- `Archive` is valid only from `Finalized` (`CannotArchiveUnlessFinalized`).
- Archived is terminal.
- `StreamRef` must not be `Guid.Empty` — enforced by `EnsureInvariants` (`OrphanedMetrics`).

## Owns

- Metrics identity, stream binding, optional recording binding, measurement window, current snapshot, status, timestamps.
- Capture / update / finalize / archive transitions.

## References

- `StreamRef` — the stream being measured.
- `RecordingRef` (optional) — when the metrics describe a specific recording.
- `MetricsSnapshot` — immutable value-object snapshot of the current measurement.

## Does not own

- Business analytics, KPIs, or revenue metrics — those live downstream in projections / analytics layers.
- Alerting rules — orchestration / observability infrastructure.
- Aggregation across many streams — projection concern; this aggregate owns per-stream metric truth only.
- The metric-collection pipeline — infrastructure.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanUpdateObservabilitySpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `ObservabilityAggregate.Capture(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
