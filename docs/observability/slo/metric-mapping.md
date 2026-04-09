# SLO → Meter → Instrument Map

**STATUS:** SCAFFOLD — frozen as of 2026-04-09.

Authoritative cross-reference between every scaffolded SLO and the
canonical instrument that measures it. **Every SLO file in this
directory MUST resolve its `Mapped instrument` field through this
table.** If you change a binding, change it here first and update the
SLO file in the same patch.

## Meter inventory (anchor)

| Meter | Defined at |
|-------|------------|
| `Whyce.Outbox` | [src/platform/host/adapters/KafkaOutboxPublisher.cs:27](../../../src/platform/host/adapters/KafkaOutboxPublisher.cs#L27), [src/platform/host/adapters/OutboxDepthSampler.cs:41](../../../src/platform/host/adapters/OutboxDepthSampler.cs#L41) |
| `Whyce.Postgres` | [src/platform/host/adapters/PostgresPoolMetrics.cs:33](../../../src/platform/host/adapters/PostgresPoolMetrics.cs#L33) |
| `Whyce.EventStore` | [src/platform/host/adapters/PostgresEventStoreAdapter.cs:47](../../../src/platform/host/adapters/PostgresEventStoreAdapter.cs#L47) |
| `Whyce.Policy` | [src/platform/host/adapters/OpaPolicyEvaluator.cs:39](../../../src/platform/host/adapters/OpaPolicyEvaluator.cs#L39) |
| `Whyce.Chain` | [src/runtime/event-fabric/ChainAnchorService.cs:43](../../../src/runtime/event-fabric/ChainAnchorService.cs#L43) |
| `Whyce.Workflow` | [src/runtime/dispatcher/WorkflowAdmissionGate.cs:38](../../../src/runtime/dispatcher/WorkflowAdmissionGate.cs#L38) |
| `Whyce.Projection.Consumer` | [src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs:28](../../../src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs#L28) |
| `Whyce.Intake` | [src/platform/host/adapters/IntakeMetrics.cs:30](../../../src/platform/host/adapters/IntakeMetrics.cs#L30) |

## Instrument inventory (per meter)

### `Whyce.Outbox`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `outbox.published` | Counter&lt;long&gt; | rows | `topic` | `KafkaOutboxPublisher.cs:28` |
| `outbox.failed` | Counter&lt;long&gt; | rows | — | `KafkaOutboxPublisher.cs:29` |
| `outbox.deadlettered` | Counter&lt;long&gt; | rows | — | `KafkaOutboxPublisher.cs:30` |
| `outbox.dlq_published` | Counter&lt;long&gt; | rows | `topic` | `KafkaOutboxPublisher.cs:31` |
| `outbox.depth` | ObservableGauge&lt;long&gt; | rows | — | `OutboxDepthSampler.cs:52` |
| `outbox.oldest_pending_age_seconds` | ObservableGauge&lt;double&gt; | s | — | `OutboxDepthSampler.cs:54` |
| `outbox.deadletter_depth` | ObservableGauge&lt;long&gt; | rows | — | `OutboxDepthSampler.cs:58` |

### `Whyce.Postgres`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `postgres.pool.acquisitions` | Counter&lt;long&gt; | calls | `pool` | `PostgresPoolMetrics.cs:36` |
| `postgres.pool.acquisition_failures` | Counter&lt;long&gt; | calls | `pool`, `reason` | `PostgresPoolMetrics.cs:39` |

### `Whyce.EventStore`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `event_store.append.advisory_lock_wait_ms` | Histogram&lt;double&gt; | ms | — | `PostgresEventStoreAdapter.cs:49` |
| `event_store.append.hold_ms` | Histogram&lt;double&gt; | ms | `outcome` (`ok`, `concurrency_conflict`, `exception`) | `PostgresEventStoreAdapter.cs:51` |
| `event_store.replay_rows` | Histogram&lt;double&gt; | rows | — | `PostgresEventStoreAdapter.cs:78` |

### `Whyce.Policy`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `policy.evaluate.duration` | Histogram&lt;double&gt; | ms | `policy_id`, `outcome` | `OpaPolicyEvaluator.cs:41` |
| `policy.evaluate.timeout` | Counter&lt;long&gt; | calls | `policy_id` | `OpaPolicyEvaluator.cs:43` |
| `policy.evaluate.breaker_open` | Counter&lt;long&gt; | calls | `policy_id` | `OpaPolicyEvaluator.cs:45` |
| `policy.evaluate.failure` | Counter&lt;long&gt; | calls | `policy_id`, `reason` | `OpaPolicyEvaluator.cs:47` |

### `Whyce.Chain`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `chain.anchor.wait_ms` | Histogram&lt;double&gt; | ms | — | `ChainAnchorService.cs:45` |
| `chain.anchor.hold_ms` | Histogram&lt;double&gt; | ms | `outcome` (`ok`, `engine_failed`, `exception`) | `ChainAnchorService.cs:47` |

### `Whyce.Workflow`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `workflow.admitted` | Counter&lt;long&gt; | workflows | — | `WorkflowAdmissionGate.cs:40` |
| `workflow.rejected` | Counter&lt;long&gt; | workflows | — | `WorkflowAdmissionGate.cs:42` |

### `Whyce.Projection.Consumer`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `consumer.consumed` | Counter&lt;long&gt; | messages | — | `GenericKafkaProjectionConsumerWorker.cs:29` |
| `consumer.handler_invoked` | Counter&lt;long&gt; | messages | — | `GenericKafkaProjectionConsumerWorker.cs:31` |
| `consumer.dlq_routed` | Counter&lt;long&gt; | messages | — | `GenericKafkaProjectionConsumerWorker.cs:30` |
| `consumer.dlq_publish_failed` | Counter&lt;long&gt; | messages | — | `GenericKafkaProjectionConsumerWorker.cs:40` |
| `projection.lag_seconds` | Histogram&lt;double&gt; | s | — | `GenericKafkaProjectionConsumerWorker.cs:68` |

### `Whyce.Intake`

| Instrument | Type | Unit | Tags | Defined at |
|------------|------|------|------|------------|
| `intake.admitted` | Counter&lt;long&gt; | requests | — | `IntakeMetrics.cs:33` |
| `intake.rejected` | Counter&lt;long&gt; | requests | — | `IntakeMetrics.cs:36` |
| `intake.queue.full` | Counter&lt;long&gt; | events | — | `IntakeMetrics.cs:39` |

---

## Forward map: SLO → instrument

| SLO ID | Instrument(s) |
|--------|---------------|
| L-1 | `policy.evaluate.duration` |
| L-2 | `chain.anchor.hold_ms` |
| L-3 | `chain.anchor.wait_ms` |
| L-4 | `event_store.append.hold_ms` |
| L-5 | `event_store.append.advisory_lock_wait_ms` |
| L-6 | `projection.lag_seconds` |
| L-7 | **UNMAPPED** — requires new runtime end-to-end histogram |
| F-1 | `outbox.failed` / (`outbox.published` + `outbox.failed`) |
| F-2 | `outbox.deadlettered` / (`outbox.published` + `outbox.deadlettered`) |
| F-3 | `event_store.append.hold_ms{outcome=concurrency_conflict}` / total |
| F-4 | `postgres.pool.acquisition_failures` / `postgres.pool.acquisitions` |
| F-5 | `policy.evaluate.timeout` / total `policy.evaluate.duration` |
| F-6 | `policy.evaluate.breaker_open` / total `policy.evaluate.duration` |
| F-7 | `policy.evaluate.failure` / total `policy.evaluate.duration` |
| F-8 | `chain.anchor.hold_ms{outcome != ok}` / total |
| F-9 | `workflow.rejected` / (`workflow.admitted` + `workflow.rejected`) |
| R-1 | `outbox.depth` (peak → baseline) |
| R-2 | `outbox.oldest_pending_age_seconds` |
| R-3 | `outbox.deadletter_depth` |
| R-4 | composite (`policy.evaluate.breaker_open` falling + `policy.evaluate.duration{outcome=success}` rising) |
| R-5 | `postgres.pool.acquisition_failures` rate-of-change |
| R-6 | `chain.anchor.hold_ms` outcome tag |
| R-7 | `projection.lag_seconds` |

## UNMAPPED gaps (recorded so they are visible)

| SLO | Reason | Resolution scope |
|-----|--------|------------------|
| L-7 (end-to-end command latency) | No runtime-level histogram exists. The closest signal is the sum of L-1 + L-2 + L-4 plus dispatcher overhead, which cannot be expressed as a single instrument. | Out of scope for §5.4 scaffold. Adding requires a new histogram on `RuntimeControlPlane.ExecuteAsync` and a new entry in this map. |
