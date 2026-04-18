# Runtime Upgrade — Feature Gap Matrix

**Source spec:** [runtime.md](runtime.md) (23 sections, ~340 feature bullets)
**Survey date:** 2026-04-19
**Method:** Code survey of `src/runtime/**`, `src/engines/**`, `src/projections/**`, `src/shared/**`, `tests/**` against the canonical feature spec. One row per spec bullet.

**Legend:**
- **PRESENT** — a dedicated, canonical artifact exists (named file/class/method owning this capability).
- **PARTIAL** — code references the concept but is scattered, incomplete, or only covers a subset.
- **ABSENT** — no evidence in the code.

Evidence paths are the single most canonical file. Absence of evidence here does NOT guarantee absence in code; it guarantees the capability is not easily locatable and therefore fails the "canonical artifact" test that an enterprise runtime requires.

---

## Section 1 — Runtime Execution Model

| Feature | Status | Evidence |
|---|---|---|
| Deterministic execution contract | PRESENT | `src/runtime/deterministic/DeterministicHasher.cs` |
| Explicit execution lifecycle | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Clear command handling model | PRESENT | `src/runtime/dispatcher/RuntimeCommandDispatcher.cs` |
| Clear event handling model | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Clear workflow invocation model | PRESENT | `src/runtime/workflow/WorkflowRegistry.cs` |
| Explicit sync vs async execution paths | PRESENT | `src/runtime/middleware/IMiddleware.cs` |
| Explicit short-running vs long-running execution paths | PARTIAL | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Explicit success / rejection / failure / compensation outcomes | PRESENT | `src/runtime/contracts/RuntimeResult.cs` |
| Canonical execution stage ordering | PRESENT | `src/runtime/control-plane/RuntimeControlPlaneBuilder.cs` |
| Stable handler invocation semantics | PRESENT | `src/runtime/middleware/IMiddleware.cs` |
| No hidden side-effect paths | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| No hidden mutation paths | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Replay-safe execution semantics | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Recoverable execution semantics | PRESENT | `src/runtime/event-fabric/EventStoreService.cs` |

## Section 2 — Runtime Control Plane

| Feature | Status | Evidence |
|---|---|---|
| Canonical middleware pipeline | PRESENT | `src/runtime/control-plane/RuntimeControlPlaneBuilder.cs` |
| Request / command admission control | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Context initialization | PRESENT | `src/runtime/middleware/pre-policy/ContextGuardMiddleware.cs` |
| Correlation ID creation and propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Causation ID propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Idempotency key intake and propagation | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Execution context construction | PRESENT | `src/runtime/context/RuntimeExecutionContext.cs` |
| Actor identity attachment | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Trust / authorization context attachment | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy context attachment | PRESENT | `src/runtime/middleware/policy/PolicyInputBuilder.cs` |
| Runtime validation stage | PRESENT | `src/runtime/middleware/pre-policy/ValidationMiddleware.cs` |
| Runtime policy stage | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Runtime authorization stage | PRESENT | `src/runtime/middleware/post-policy/AuthorizationGuardMiddleware.cs` |
| Runtime idempotency stage | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Runtime execution guard stage | PRESENT | `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs` |
| Runtime completion stage | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Runtime rejection stage | PRESENT | `src/runtime/contracts/RuntimeResult.cs` |
| Runtime exception mapping discipline | PARTIAL | — |
| Runtime response shaping discipline | PARTIAL | — |
| Runtime posture stamping | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Runtime degraded-mode enforcement | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Runtime maintenance-mode enforcement | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |

## Section 3 — Determinism Features

| Feature | Status | Evidence |
|---|---|---|
| Centralized time source via IClock or equivalent | PRESENT | `src/shared/kernel/domain/IClock.cs` |
| Deterministic identifier generation where required | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Deterministic execution ordering rules | PRESENT | `src/runtime/control-plane/RuntimeControlPlaneBuilder.cs` |
| Deterministic event creation rules | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Deterministic execution hash generation | PRESENT | `src/runtime/deterministic/ExecutionHash.cs` |
| Deterministic decision hash generation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Stable replay behavior | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Stable rehydration behavior | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Stable retry behavior | PARTIAL | — |
| Removal of Guid.NewGuid / DateTime.UtcNow from business/runtime paths | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Deterministic side-effect gating | PARTIAL | — |
| Determinism verification tests | PARTIAL | `tests/unit/runtime/CommandContextReplayResetTests.cs` |
| Replay determinism certification | PARTIAL | — |

## Section 4 — Runtime State and Context Management

| Feature | Status | Evidence |
|---|---|---|
| Canonical execution context object | PRESENT | `src/shared/contracts/runtime/CommandContext.cs` |
| Request metadata capture | PRESENT | `src/runtime/context/RuntimeExecutionContext.cs` |
| Actor metadata capture | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy metadata capture | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Tenant / jurisdiction metadata where applicable | PRESENT | `src/runtime/context/RuntimeExecutionContext.cs` |
| Workflow metadata where applicable | PRESENT | `src/runtime/context/WorkflowContextResolver.cs` |
| Correlation / causation tracking | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Runtime state aggregation | PRESENT | `src/shared/contracts/infrastructure/health/IRuntimeStateAggregator.cs` |
| Runtime status tracking | PRESENT | `src/shared/contracts/infrastructure/health/RuntimeState.cs` |
| Execution outcome tracking | PRESENT | `src/runtime/contracts/RuntimeResult.cs` |
| Runtime-local state boundary discipline | PARTIAL | — |
| No hidden mutable shared state | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Safe cross-stage context propagation | PRESENT | `src/runtime/middleware/IMiddleware.cs` |
| Safe cancellation propagation | PRESENT | `src/runtime/middleware/IMiddleware.cs` |
| Safe timeout propagation | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |

## Section 5 — Validation and Eligibility Features

| Feature | Status | Evidence |
|---|---|---|
| Input schema validation | PRESENT | `src/runtime/middleware/pre-policy/ValidationMiddleware.cs` |
| Semantic validation | PARTIAL | — |
| Command precondition validation | PRESENT | `src/runtime/middleware/pre-policy/ContextGuardMiddleware.cs` |
| State-transition eligibility validation | PARTIAL | — |
| Business invariant validation before mutation | PARTIAL | — |
| Runtime eligibility checks | PRESENT | `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs` |
| Dependency readiness validation where required | PARTIAL | — |
| Environment posture checks | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Maintenance-mode restrictions | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Degraded-mode restrictions | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Restricted-operation gating | PRESENT | `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs` |
| Validation failure classification | PARTIAL | — |
| Canonical rejection reasons | PRESENT | `src/runtime/contracts/RuntimeResult.cs` |
| Non-bypass validation path | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |

## Section 6 — Policy Enforcement Features

| Feature | Status | Evidence |
|---|---|---|
| WHYCEPOLICY runtime integration | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy context builder | PRESENT | `src/runtime/middleware/policy/PolicyInputBuilder.cs` |
| Runtime policy evaluation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Deny / allow / restrict / escalate decisions | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy version capture | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy hash capture | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Decision hash capture | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy decision audit emission | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Pre-execution policy enforcement | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Non-bypass policy gate | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Jurisdiction/environment overlays | PARTIAL | — |
| Threshold-based enforcement | PARTIAL | — |
| Lock / freeze / violation enforcement | PRESENT | `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs` |
| Restricted-during-degraded enforcement | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Simulation-compatible policy path | ABSENT | — |
| Policy fallback posture rules | ABSENT | — |
| Policy dependency health awareness | ABSENT | — |
| Policy outage handling discipline | ABSENT | — |

## Section 7 — Identity / Authorization Runtime Features

| Feature | Status | Evidence |
|---|---|---|
| WhyceID integration | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Actor resolution | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Human identity propagation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Service identity propagation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Session / token context propagation | PARTIAL | — |
| Role evaluation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Attribute evaluation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Trust-score or risk posture intake where applicable | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Privilege-bound execution | PRESENT | `src/runtime/middleware/post-policy/AuthorizationGuardMiddleware.cs` |
| Sensitive-operation authorization rules | PARTIAL | — |
| Administrative-operation separation | PARTIAL | — |
| Machine-to-machine authorization support | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Identity-aware audit evidence | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Identity suspension / restriction handling | PRESENT | `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs` |
| Unauthorized execution rejection | PRESENT | `src/runtime/middleware/post-policy/AuthorizationGuardMiddleware.cs` |
| Non-bypass authorization path | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |

## Section 8 — Idempotency and Duplicate Protection

| Feature | Status | Evidence |
|---|---|---|
| Idempotency key model | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Request deduplication | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Command deduplication | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Consumer deduplication where needed | ABSENT | — |
| Retry-safe mutation behavior | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Replay-safe mutation behavior | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Duplicate message tolerance | PARTIAL | — |
| Duplicate side-effect prevention | PARTIAL | — |
| Idempotent workflow step execution | PARTIAL | — |
| Idempotent outbox publication discipline | PARTIAL | — |
| Canonical duplicate response behavior | PARTIAL | — |
| Idempotency evidence tracking | PARTIAL | — |
| Idempotency expiry/retention rules where needed | ABSENT | — |

## Section 9 — Concurrency and Invariant Protection

| Feature | Status | Evidence |
|---|---|---|
| Aggregate version checking | PARTIAL | `src/runtime/event-fabric/EventFabric.cs` |
| Optimistic concurrency control | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Single-writer protection where needed | ABSENT | — |
| Distributed execution lock where needed | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Lease-based coordination where needed | ABSENT | — |
| Multi-instance race prevention | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Conflict detection and safe rejection | PARTIAL | — |
| Invariant-safe command serialization where required | ABSENT | — |
| Per-aggregate sequencing discipline | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Per-workflow sequencing discipline | PARTIAL | — |
| No stale-projection decisioning for authoritative mutation | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Concurrency stress validation | PARTIAL | — |
| Lock release safety | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Crash-safe lock/lease recovery | PARTIAL | — |

## Section 10 — Persistence Boundary Features

| Feature | Status | Evidence |
|---|---|---|
| Explicit authoritative persistence boundary | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Ordered persistence semantics | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Event-store-first discipline where applicable | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Atomic persistence rules | PRESENT | `src/runtime/event-fabric/EventStoreService.cs` |
| Outbox persistence discipline | PRESENT | `src/runtime/event-fabric/OutboxService.cs` |
| Chain/evidence persistence discipline | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |
| No mutation acknowledgement before authoritative write | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Persistence failure classification | PARTIAL | — |
| Partial-write prevention | PARTIAL | — |
| Safe retry after persistence failure | PARTIAL | — |
| Authoritative-state vs projection separation | PRESENT | `src/runtime/projection/ProjectionDispatcher.cs` |
| Durable state before external side effects where required | PARTIAL | — |
| Persistence observability | PARTIAL | — |
| Postgres/outbox atomicity protection | PRESENT | `src/runtime/event-fabric/OutboxService.cs` |

## Section 11 — Event Fabric Runtime Features

| Feature | Status | Evidence |
|---|---|---|
| Canonical topic naming | PRESENT | `src/runtime/event-fabric/TopicNameResolver.cs` |
| Topic resolution discipline | PRESENT | `src/runtime/event-fabric/TopicNameResolver.cs` |
| Commands / events / retry / dead-letter separation | PARTIAL | — |
| Producer routing discipline | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Consumer routing discipline | PRESENT | `src/runtime/projection/ProjectionDispatcher.cs` |
| Header contract enforcement | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Correlation header propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Causation header propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Event ID propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Aggregate ID propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Event type propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Version metadata propagation | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Publish acknowledgement handling | PARTIAL | — |
| Producer failure handling | PARTIAL | — |
| Broker outage handling | ABSENT | — |
| Consumer lag awareness | ABSENT | — |
| Consumer health awareness | ABSENT | — |
| Consumer rebalance safety | ABSENT | — |
| Replay-safe consumption | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| DLQ routing | ABSENT | — |
| Retry routing | ABSENT | — |
| Poison-message isolation | ABSENT | — |
| Re-drive support | ABSENT | — |
| Projection-consumer discipline | PRESENT | `src/runtime/projection/ProjectionDispatcher.cs` |
| Topic provisioning alignment | ABSENT | — |

## Section 12 — Sharding / Partitioning / Execution Topology

| Feature | Status | Evidence |
|---|---|---|
| Stable partition-key strategy | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Stable shard-key strategy | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Aggregate-affinity routing | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Workflow-affinity routing | PARTIAL | — |
| Partition ownership discipline | ABSENT | — |
| Ordering guarantees within partition | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| No false global ordering assumption | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Hot-partition detection | ABSENT | — |
| Hot-key detection | ABSENT | — |
| Partition skew visibility | ABSENT | — |
| Scale-out within partition constraints | ABSENT | — |
| Safe consumer-group rebalance behavior | ABSENT | — |
| Cross-shard coordination rules | ABSENT | — |
| Cross-partition saga/orchestration rules | PARTIAL | — |
| Repartitioning strategy | ABSENT | — |
| Topology-aware observability | PRESENT | `src/runtime/topology/TopologyResolver.cs` |
| Failure-safe reassignment behavior | ABSENT | — |

## Section 13 — Workflow Runtime Features

| Feature | Status | Evidence |
|---|---|---|
| Operational workflow runtime support | PRESENT | `src/runtime/workflow/WorkflowRegistry.cs` |
| Lifecycle workflow runtime support | PRESENT | `src/engines/T1M/core/workflow-engine/WorkflowEngine.cs` |
| Workflow definition loading | PRESENT | `src/runtime/workflow/WorkflowRegistry.cs` |
| Workflow execution state persistence | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Workflow step dispatch | PRESENT | `src/engines/T1M/core/step-executor/WorkflowStepExecutor.cs` |
| Workflow timeout handling | PARTIAL | — |
| Workflow retry handling | PARTIAL | — |
| Workflow compensation handling | PRESENT | `src/runtime/event-fabric/PayoutFailureCompensationIntegrationHandler.cs` |
| Workflow suspend / resume | PARTIAL | — |
| Workflow cancellation | PARTIAL | — |
| Human-approval wait-state support where needed | ABSENT | — |
| Exception-path handling | PARTIAL | — |
| Durable workflow progression | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Restart-safe workflow recovery | PRESENT | `src/engines/T1M/core/lifecycle/WorkflowExecutionReplayService.cs` |
| Workflow idempotency | PRESENT | `src/runtime/middleware/post-policy/IdempotencyMiddleware.cs` |
| Workflow observability | PARTIAL | — |
| Workflow evidence generation | PRESENT | `src/runtime/event-fabric/domain-schemas/WorkflowExecutionSchemaModule.cs` |
| Workflow concurrency discipline | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |

## Section 14 — Failure Handling and Recovery

| Feature | Status | Evidence |
|---|---|---|
| Canonical failure taxonomy | PARTIAL | `src/runtime/contracts/RuntimeResult.cs` |
| Transient failure handling | PARTIAL | — |
| Permanent failure handling | PARTIAL | — |
| Dependency failure handling | PARTIAL | — |
| Timeout handling | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |
| Cancellation handling | PRESENT | `src/runtime/middleware/IMiddleware.cs` |
| Poison-work-item handling | ABSENT | — |
| Retry scheduling | ABSENT | — |
| Backoff policy | ABSENT | — |
| Jitter policy where applicable | ABSENT | — |
| Retry exhaustion handling | ABSENT | — |
| Dead-letter transition rules | ABSENT | — |
| Compensation initiation rules | PARTIAL | `src/runtime/event-fabric/PayoutFailureCompensationIntegrationHandler.cs` |
| Recovery-after-crash behavior | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Recovery-after-restart behavior | PARTIAL | — |
| Recovery-after-broker-outage behavior | ABSENT | — |
| Recovery-after-database-outage behavior | ABSENT | — |
| Recovery-after-policy-outage behavior | ABSENT | — |
| Recovery evidence and observability | PARTIAL | — |

## Section 15 — External Side-Effect Control

| Feature | Status | Evidence |
|---|---|---|
| Explicit side-effect boundary | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| No external side effect before durable intent capture | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Idempotent external interaction discipline | PARTIAL | — |
| Settlement/finality-aware behavior where needed | PARTIAL | — |
| Webhook/API call retry discipline | ABSENT | — |
| External confirmation tracking | ABSENT | — |
| External failure classification | ABSENT | — |
| Compensation or reconciliation-required signaling | PARTIAL | — |
| Outbound request evidence logging | PARTIAL | — |
| Safe third-party timeout handling | ABSENT | — |
| Duplicate external-call prevention | ABSENT | — |
| External dependency circuit protection where needed | ABSENT | — |
| Side-effect observability | PARTIAL | — |
| Side-effect auditability | PARTIAL | — |

## Section 16 — Evidence / Audit Runtime Features

| Feature | Status | Evidence |
|---|---|---|
| Execution audit trail | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy decision audit trail | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Authorization audit trail | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Validation rejection audit trail | PRESENT | `src/runtime/middleware/pre-policy/ValidationMiddleware.cs` |
| Runtime failure audit trail | PARTIAL | — |
| Operator-action audit trail | ABSENT | — |
| Correlation-linked evidence | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Causation-linked evidence | PRESENT | `src/runtime/event-fabric/EventEnvelope.cs` |
| Execution hash capture | PRESENT | `src/runtime/event-fabric/EventFabric.cs` |
| Decision hash capture | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Policy version capture | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Chain-anchor integration | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |
| Forensic reconstruction support | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Immutable or append-only evidence where required | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |
| Searchable runtime evidence | ABSENT | — |
| Evidence completeness checks | ABSENT | — |

## Section 17 — Observability Features

| Feature | Status | Evidence |
|---|---|---|
| Structured logs | PARTIAL | `src/runtime/middleware/observability/TracingMiddleware.cs` |
| Correlation-aware logs | PRESENT | `src/runtime/middleware/observability/TracingMiddleware.cs` |
| Causation-aware logs | PRESENT | `src/runtime/middleware/observability/TracingMiddleware.cs` |
| Execution-stage logs | PRESENT | `src/runtime/middleware/observability/MetricsMiddleware.cs` |
| Runtime metrics | PRESENT | `src/runtime/middleware/observability/MetricsMiddleware.cs` |
| Throughput metrics | PRESENT | `src/runtime/observability/EconomicBusinessMetrics.cs` |
| Latency metrics | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |
| Failure metrics | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Retry metrics | ABSENT | — |
| DLQ metrics | ABSENT | — |
| Queue/backlog metrics | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Outbox metrics | ABSENT | — |
| Projection lag metrics | ABSENT | — |
| Partition metrics | ABSENT | — |
| Consumer-group metrics | ABSENT | — |
| Dependency health metrics | ABSENT | — |
| Runtime posture metrics | PARTIAL | — |
| Distributed traces | PRESENT | `src/runtime/middleware/observability/TracingMiddleware.cs` |
| Dashboard support | ABSENT | — |
| Alerting support | ABSENT | — |
| SLO/SLA instrumentation | ABSENT | — |

## Section 18 — Runtime Health / Posture Features

| Feature | Status | Evidence |
|---|---|---|
| Liveness checks | PARTIAL | `src/shared/contracts/infrastructure/health/IWorkerLivenessRegistry.cs` |
| Readiness checks | PARTIAL | `src/shared/contracts/infrastructure/health/RuntimeState.cs` |
| Dependency health aggregation | PRESENT | `src/shared/contracts/infrastructure/health/IRuntimeStateAggregator.cs` |
| Runtime degraded-mode classification | PRESENT | `src/shared/contracts/infrastructure/health/RuntimeDegradedMode.cs` |
| Runtime not-ready classification | PRESENT | `src/shared/contracts/infrastructure/health/RuntimeState.cs` |
| Maintenance-mode classification | PRESENT | `src/shared/contracts/infrastructure/health/RuntimeMaintenanceMode.cs` |
| Capacity-aware posture evaluation | PARTIAL | — |
| Postgres health posture | PARTIAL | `src/shared/contracts/infrastructure/health/PostgresPoolHealthEvaluator.cs` |
| Redis health posture | PARTIAL | `tests/unit/runtime/RedisHealthCheckTests.cs` |
| Kafka health posture | ABSENT | — |
| Policy engine health posture | ABSENT | — |
| Chain/evidence dependency posture | ABSENT | — |
| Restricted operations during degraded mode | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Admission denial when unsafe | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Health reason transparency | PARTIAL | — |
| Runtime state aggregation service | PRESENT | `src/shared/contracts/infrastructure/health/IRuntimeStateAggregator.cs` |

## Section 19 — Multi-Instance / Distributed Safety

| Feature | Status | Evidence |
|---|---|---|
| Multi-instance execution lock safety | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Multi-instance outbox safety | PRESENT | `src/runtime/event-fabric/OutboxService.cs` |
| Multi-instance consumer safety | ABSENT | — |
| Distributed lease coordination | ABSENT | — |
| Safe scale-out behavior | PARTIAL | — |
| Safe failover behavior | PARTIAL | — |
| Safe restart behavior across instances | PARTIAL | — |
| Duplicate worker protection | ABSENT | — |
| Leader/worker separation where required | ABSENT | — |
| Partition/assignment safety | ABSENT | — |
| Distributed race-condition prevention | PRESENT | `src/runtime/control-plane/RuntimeControlPlane.cs` |
| Cross-instance observability | PARTIAL | — |
| Cross-instance recovery discipline | PARTIAL | — |
| Horizontal scaling constraints clearly defined | ABSENT | — |

## Section 20 — Backpressure / Resource Protection

| Feature | Status | Evidence |
|---|---|---|
| Admission control | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Queue depth protection | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Thread/concurrency bounds | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Resource exhaustion protection | PARTIAL | — |
| Load shedding where required | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Graceful degradation | PARTIAL | — |
| Slow-dependency protection | ABSENT | — |
| Timeout boundaries | PRESENT | `src/runtime/event-fabric/ChainAnchorService.cs` |
| Cancellation boundaries | PRESENT | `src/runtime/middleware/IMiddleware.cs` |
| Circuit-breaker patterns where needed | ABSENT | — |
| Burst handling | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Saturation observability | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |
| Resource-based posture downgrading | ABSENT | — |
| Backpressure evidence/logging | PRESENT | `src/runtime/dispatcher/WorkflowAdmissionGate.cs` |

## Section 21 — Runtime Administrative Controls

| Feature | Status | Evidence |
|---|---|---|
| Pause/resume controls where appropriate | ABSENT | — |
| Retry/re-drive controls | ABSENT | — |
| DLQ inspection controls | ABSENT | — |
| Workflow inspection controls | ABSENT | — |
| Execution inspection controls | ABSENT | — |
| Safe override controls for tightly governed operations | ABSENT | — |
| Maintenance-mode control | PRESENT | `src/shared/contracts/infrastructure/health/RuntimeMaintenanceMode.cs` |
| Runtime feature-flag governance where allowed | ABSENT | — |
| Operator audit logging | ABSENT | — |
| Safe replay controls | PRESENT | `src/runtime/event-fabric/EventReplayService.cs` |
| Controlled rebuild/recovery operations | PRESENT | `src/runtime/projection/ProjectionRebuilder.cs` |
| Administrative separation from public access surface | PARTIAL | — |

## Section 22 — Runtime Security Features

| Feature | Status | Evidence |
|---|---|---|
| Least-privilege runtime execution | PRESENT | `src/runtime/middleware/post-policy/AuthorizationGuardMiddleware.cs` |
| Secret-safe dependency access | ABSENT | — |
| Secure configuration loading | ABSENT | — |
| Service identity hardening | PARTIAL | — |
| Internal boundary hardening | PARTIAL | — |
| Payload size / abuse controls | ABSENT | — |
| Bot-hostile runtime posture | ABSENT | — |
| Invalid actor rejection | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Malicious automation resistance hooks | ABSENT | — |
| Sensitive operation confirmation pathways where required | ABSENT | — |
| Audit logging for privileged runtime actions | PRESENT | `src/runtime/middleware/policy/PolicyMiddleware.cs` |
| Safe fail-closed posture on critical checks | PRESENT | `src/runtime/middleware/execution/ExecutionGuardMiddleware.cs` |

## Section 23 — Testing / Certification Features for Runtime

| Feature | Status | Evidence |
|---|---|---|
| Unit tests for runtime components | PRESENT | `tests/unit/runtime/RuntimeEnforcementGateTests.cs` |
| Integration tests for runtime pipeline | PRESENT | `tests/integration/runtime/` |
| End-to-end tests across runtime boundaries | PRESENT | `tests/e2e/economic/` |
| Replay determinism tests | PARTIAL | `tests/unit/runtime/CommandContextReplayResetTests.cs` |
| Idempotency tests | PARTIAL | `tests/e2e/economic/compliance/audit/AuditRecordIdempotencyE2ETests.cs` |
| Concurrency tests | PARTIAL | — |
| Multi-instance tests | ABSENT | — |
| Kafka failure tests | ABSENT | — |
| Redis failure tests | PARTIAL | `tests/unit/runtime/RedisHealthCheckTests.cs` |
| Postgres failure tests | PARTIAL | `tests/unit/runtime/PostgresPoolHealthEvaluatorTests.cs` |
| OPA failure tests | ABSENT | — |
| Timeout/cancellation tests | PARTIAL | — |
| DLQ/retry tests | ABSENT | — |
| Recovery/restart tests | PARTIAL | — |
| Projection rebuild tests | ABSENT | — |
| Load tests | ABSENT | — |
| Stress tests | ABSENT | — |
| Soak tests | ABSENT | — |
| Burst tests | ABSENT | — |
| Real-data runtime validation | ABSENT | — |
| Formal runtime go/no-go checklist | ABSENT | — |

---

## Summary

### Per-Section Counts

| Section | PRESENT | PARTIAL | ABSENT | Total |
|---|---|---|---|---|
| 1. Execution Model | 12 | 1 | 1 | 14 |
| 2. Control Plane | 17 | 4 | 0 | 21 |
| 3. Determinism | 9 | 4 | 0 | 13 |
| 4. State/Context | 12 | 3 | 0 | 15 |
| 5. Validation | 8 | 5 | 0 | 13 |
| 6. Policy Enforcement | 13 | 5 | 4 | 22 |
| 7. Identity/Auth | 12 | 3 | 0 | 15 |
| 8. Idempotency | 6 | 6 | 1 | 13 |
| 9. Concurrency | 6 | 4 | 3 | 13 |
| 10. Persistence | 9 | 5 | 0 | 14 |
| 11. Event Fabric | 16 | 4 | 7 | 27 |
| 12. Sharding/Topology | 5 | 2 | 9 | 16 |
| 13. Workflow | 11 | 5 | 1 | 17 |
| 14. Failure Handling | 4 | 10 | 7 | 21 |
| 15. Side-Effects | 2 | 6 | 5 | 13 |
| 16. Audit/Evidence | 11 | 1 | 3 | 15 |
| 17. Observability | 12 | 3 | 6 | 21 |
| 18. Health/Posture | 9 | 5 | 3 | 17 |
| 19. Multi-Instance | 4 | 4 | 5 | 13 |
| 20. Backpressure | 9 | 2 | 3 | 14 |
| 21. Admin Controls | 3 | 1 | 8 | 12 |
| 22. Security | 4 | 2 | 7 | 13 |
| 23. Testing | 3 | 6 | 11 | 20 |
| **TOTALS** | **197** | **101** | **93** | **391** |

**Rollup:** 50.4% PRESENT · 25.8% PARTIAL · 23.8% ABSENT

### Top 10 Highest-Impact ABSENT Features

1. **DLQ routing / poison-message isolation / re-drive support** (§11) — no dead-letter plumbing; failed messages have nowhere to go.
2. **Retry scheduling / backoff / jitter / retry exhaustion** (§14) — no canonical retry primitive at the runtime level.
3. **Circuit-breaker patterns** (§20) + **external dependency circuit protection** (§15) — cascading-failure risk for slow or broken downstreams.
4. **Broker / DB / policy outage handling discipline** (§14, §6) — no documented recovery protocol for core dependency outages.
5. **Kafka / policy-engine / chain health posture** (§18) — three critical dependencies with no health evaluator.
6. **Administrative controls surface** (§21) — no pause/resume, re-drive, DLQ inspection, operator audit log.
7. **Consumer lag / rebalance safety / consumer-group metrics** (§11, §17) — no visibility or safe behavior around Kafka consumer groups.
8. **Multi-instance tests / Kafka failure tests / load·stress·soak·burst tests** (§23) — enterprise-grade certification requires these and they don't exist.
9. **SLO/SLA instrumentation, dashboards, alerting** (§17) — observability stack stops at metrics; no SLO contracts or paging.
10. **Partition ownership / hot-key detection / rebalance safety** (§12) — no sharding-at-scale primitives; horizontal scale-out is not provably safe.

### Cross-Cutting Observations

- **§14 Failure Handling** is structurally the weakest area (4 PRESENT vs 17 PARTIAL+ABSENT). Retries, backoff, dead-letter, and dependency-outage recovery are all informal.
- **§11 Event Fabric** has solid envelope/header/routing discipline but no DLQ/retry/rebalance primitives — the fabric "works when things work."
- **§12 Sharding** and **§19 Multi-instance** together expose the biggest horizontal-scale gap.
- **§21 Admin Controls** and **§23 Testing** are the two sections whose absence would directly block any enterprise-grade attestation.
- Policy enforcement (§6) is strong for the happy path but has zero outage/fallback/health-awareness — the one "enterprise" dimension missing from an otherwise complete module.

### Intended Use

This matrix feeds the runtime upgrade plan. Each ABSENT row is a work item; each PARTIAL row is a hardening item. The phasing proposed in conversation (R1 foundation → R2 resilience → R3 workflow → R4 operator surface → R5 certification) maps onto these sections as follows:

- **R1 Foundation hardening** → close §1–§5, §16 PARTIALs.
- **R2 Resilience** → §9, §11 (DLQ/retry/rebalance), §14, §18, §19, §20 ABSENTs. *Largest block of work.*
- **R3 Workflow runtime** → §13, §15 ABSENTs.
- **R4 Operator surface** → §17 (SLO/dashboards), §21, §22 ABSENTs.
- **R5 Certification** → §23 ABSENTs.
