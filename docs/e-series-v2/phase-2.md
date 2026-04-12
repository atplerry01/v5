# PHASE 2 -- INTELLIGENCE, OBSERVABILITY & IDENTITY v2

## SCOPE: E13 - E16

## PRE-REQ: Phase 1 fully proven (E2E across all active domains)

## VERSION: 2.0 -- Updated with infrastructure insights from Kanban validation

---

# OBJECTIVE

Transform the system from "it works" into "it is measurable, traceable,
diagnosable, secure, and optimised." Phase 2 does NOT add new domains. It
elevates existing working domains into production-grade operations.

---

# IMPLEMENTATION FLOW

```
E13 Observability (foundation -- must come first)
  E14 Governance Assist (alerts, dashboards, anomaly detection)
  E16 Identity Integration (identity-aware execution)
  E15 Optimization (data-driven tuning)
  PHASE 2 PROOF
```

Optimization comes AFTER visibility + identity. You cannot optimize what you
cannot measure.

---

# E13 -- OBSERVABILITY

## Already In Place (from Phase 1 Kanban validation)

The following observability infrastructure was verified during Kanban E2E:

* CorrelationId propagated end-to-end (API -> EventStore -> WhyceChain -> Outbox -> Kafka -> Projection)
* Prometheus metrics:
  - postgres.pool.acquisitions (pool=event-store/chain/projections)
  - consumer.consumed / consumer.dlq_routed per topic
  - projection.lag_seconds (broker timestamp vs write time)
  - outbox.published / outbox.failed / outbox.deadlettered
* Worker liveness registry (IWorkerLivenessRegistry)
* Structured exception types (ConcurrencyConflictException -> 409, OutboxSaturatedException -> 503)

## Still Needed

### 1. Distributed Tracing (TraceId + SpanId)

Every request should carry OpenTelemetry trace context across:

```
API -> Systems -> Runtime -> Middleware -> Engine -> EventStore -> Chain -> Outbox -> Projection
```

Current state: CorrelationId exists but is not an OTEL trace. Need to bridge
ASP.NET ActivitySource with the runtime pipeline.

### 2. Complete Metrics Coverage

Add missing metrics:
* Command execution duration (p50/p95/p99)
* Policy evaluation latency
* Chain anchor latency and circuit breaker state
* Engine execution time per command type
* Request throughput (RPS) per endpoint

### 3. Structured Logging

Every log entry must include:
* correlationId
* commandId / commandType
* domain route (classification/context/domain)
* actorId
* outcome (success/failure/denied)

Currently: log output is minimal. Need structured JSON logging with semantic fields.

### 4. Kafka Consumer Monitoring

* Consumer group lag per topic (via kafka-exporter, already deployed)
* Dead letter queue depth as a gauge
* Projection rebuild progress tracking

## Where

```
src/platform/host/observability/     (OTEL configuration, exporters)
src/runtime/middleware/               (per-middleware metrics)
src/shared/contracts/observability/   (metric contracts)
```

---

# E14 -- GOVERNANCE ASSIST

## Alerting Rules (Prometheus/Grafana)

Define alerts for:
* Outbox backlog > high-water-mark (10,000 default)
* Consumer DLQ rate spike (> 1% of consumed)
* Projection lag > 30 seconds
* Health endpoint returns non-HEALTHY
* Chain anchor circuit breaker tripped (5 consecutive failures)

## Admin Dashboards

Build Grafana dashboards showing:
* Domain execution stats (commands/s, success rate per domain)
* Event pipeline health (outbox depth, publish rate, consumer lag)
* Policy decisions (allow/deny rate per domain)
* Infrastructure health (Postgres pool utilization, Redis connections)

## Anomaly Detection (Basic)

* Repeated 409 OCC conflicts on same aggregate (contention hotspot)
* Sustained policy denial rate increase (possible auth misconfiguration)
* Outbox drain rate falling behind enqueue rate (capacity issue)

Already in place from Kanban:
* OutboxDepthSampler with high-water-mark refusal
* Worker liveness with staleness detection
* DLQ counters per topic/reason

---

# E16 -- IDENTITY INTEGRATION

## Current State (from Kanban validation)

* ActorId on CommandContext (set to "system" by SystemIntentDispatcher)
* TenantId on CommandContext (set to "default")
* PolicyId on CommandContext (set to "whyce-policy-default")
* OPA evaluates identity.role for RBAC decisions
* Identity resolution occurs in PolicyMiddleware

## Still Needed

### 1. Real Identity Context Injection

Replace hardcoded "system" / "default" with:
* JWT/API-key authentication at API layer
* Identity resolution middleware extracts ActorId, Roles, TenantId
* TrustScore computation from identity attributes

### 2. Role Enforcement End-to-End

* API layer validates authentication token
* PolicyMiddleware enriches CommandContext with resolved identity
* OPA receives real role/trust data for policy decisions
* Audit trail records actual actor identity

### 3. Session / Device Validation

* Session validity checks (not expired, not revoked)
* Device trust signals (if applicable)

---

# E15 -- OPTIMIZATION

## Baseline Metrics (from Kanban load test)

* 20 sequential creates: all HTTP 200, system stable
* 5 concurrent creates: 4 success, 1 expected 409 OCC
* Outbox drains within seconds for published messages
* No deadlocks observed

## Optimization Targets

### 1. Connection Pool Tuning

Already instrumented (postgres.pool.acquisitions). Use metrics to:
* Right-size pool limits per pool (event-store, chain, projections)
* Detect acquisition contention

### 2. Kafka Partition Strategy

* Events topic: 3 partitions (keyed by AggregateId for ordering)
* Adjust partition count based on consumer throughput needs
* Consumer group rebalancing strategy

### 3. Projection Performance

* Read model query optimization (indexes on aggregate_id, correlation_id)
* Consider caching hot projections in Redis
* Batch projection writes for high-throughput scenarios

### 4. Outbox Publisher Tuning

* Batch size optimization (currently processes all pending)
* FOR UPDATE SKIP LOCKED enables multi-instance publishing
* Exponential backoff caps at 300s -- may need adjustment per domain

---

# PHASE 2 PROOF

A system is Phase 2 complete only if:

## Observability
* [ ] Any request can be traced end-to-end by correlationId
* [ ] Grafana dashboards show live domain execution metrics
* [ ] Logs are structured, queryable, and include identity context

## Governance Assist
* [ ] Alerts fire on outbox backlog, DLQ spike, projection lag
* [ ] System health visible at a glance
* [ ] Failures detectable before user impact

## Identity
* [ ] Every request carries real ActorId (not "system")
* [ ] Unauthorized requests rejected at policy layer
* [ ] Audit trail records actual actor identity

## Optimization
* [ ] Latency baselines established (p50/p95/p99)
* [ ] Connection pool utilization within bounds
* [ ] No regression from Phase 1 baseline

---

# CRITICAL RULE

Do NOT start Phase 2 until Phase 1 is fully proven end-to-end.
Otherwise you will optimize broken flows and observe incomplete systems.
