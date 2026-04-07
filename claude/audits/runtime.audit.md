# RUNTIME AUDIT — WBSM v3

```
AUDIT ID:       RUNTIME-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the runtime layer to verify it functions as the sole control plane for the WBSM v3 system. The runtime layer is responsible for middleware registration, command routing, event dispatch, projection triggering, and cross-cutting concern management. It must contain ZERO business logic.

This audit MUST detect:

* Business logic leakage into runtime
* Missing middleware registrations
* Command routing bypasses
* Event dispatch irregularities
* Projection triggering failures
* Configuration non-compliance

---

## SCOPE

```
src/runtime/              -> all runtime artifacts
src/runtime/controlplane/ -> control plane implementation
src/runtime/middleware/    -> middleware pipeline
src/runtime/projection/   -> projection triggering
src/runtime/routing/      -> command/event routing
src/runtime/config/       -> runtime configuration
```

Excluded: `src/domain/`, `src/engines/`, `src/systems/`, `src/platform/`, `infrastructure/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Engine persistence attempt, engine publishing event externally, engine calling infra, runtime bypass, business logic in runtime, routing bypass, missing control plane | Blocks deployment |
| HIGH | Missing middleware, incomplete event dispatch, projection gap | Must fix before merge |
| MEDIUM | Configuration hardcoding, missing correlation ID propagation | Fix within sprint |
| LOW | Missing telemetry hook, non-standard naming | Fix at convenience |

---

## GLOBAL RULE: PROJECTION LAYER AUTHORITY

* `src/projections/` = DOMAIN PROJECTION LAYER (CQRS READ MODELS)
* `src/runtime/projection/` = INTERNAL EXECUTION SUPPORT ONLY

MANDATORY:

* Domain projections:
  * consume EVENTS only
  * produce READ MODELS only
  * exposed via platform APIs
* Runtime projections:
  * NOT exposed externally
  * support execution only (routing, orchestration, indexing)

---

## AUDIT DIMENSIONS

### RDIM-01: Runtime as Sole Control Plane

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | Runtime implements `IRuntimeControlPlane` interface | CRITICAL |
| CHECK-01.2 | All engine invocations pass through runtime control plane | CRITICAL |
| CHECK-01.3 | No alternative entry points to engines exist outside runtime | CRITICAL |
| CHECK-01.4 | Runtime is the only layer with direct engine references | HIGH |

### RDIM-02: Middleware Registration

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Authentication middleware registered in pipeline | HIGH |
| CHECK-02.2 | Authorization middleware registered in pipeline | HIGH |
| CHECK-02.3 | Correlation ID middleware registered in pipeline | HIGH |
| CHECK-02.4 | Logging middleware registered in pipeline | MEDIUM |
| CHECK-02.5 | Middleware execution order is deterministic and documented | HIGH |

### RDIM-03: Command Routing

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | All commands dispatched through command router | CRITICAL |
| CHECK-03.2 | Command routing maps to correct engine tier (T0U for policy, T1M for workflow, T2E for execution) | CRITICAL |
| CHECK-03.3 | Unknown commands rejected with typed error | HIGH |
| CHECK-03.4 | Command validation occurs before routing | HIGH |

### RDIM-04: Event Dispatch

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Domain events dispatched to event bus after aggregate persistence | CRITICAL |
| CHECK-04.2 | Event dispatch is transactional (outbox pattern) | CRITICAL |
| CHECK-04.3 | Event ordering preserved per aggregate | HIGH |
| CHECK-04.4 | Failed event dispatch triggers retry mechanism | HIGH |
| CHECK-04.5 | Dead letter handling configured for poison events | HIGH |

### RDIM-05: Projection Triggering

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | Projections triggered by events only (not commands) | CRITICAL |
| CHECK-05.2 | Projection rebuild mechanism available | HIGH |
| CHECK-05.3 | Projection handlers are idempotent | HIGH |
| CHECK-05.4 | Projection lag is observable via metrics | MEDIUM |

### RDIM-06: Zero Business Logic

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | No domain aggregate references in runtime | CRITICAL |
| CHECK-06.2 | No domain service calls in runtime | CRITICAL |
| CHECK-06.3 | No business rules or conditional logic based on domain state | CRITICAL |
| CHECK-06.4 | No domain value object creation in runtime | HIGH |
| CHECK-06.5 | Runtime performs orchestration, not computation | HIGH |

### RDIM-07: Runtime Configuration Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | All configuration values externalized (not hardcoded) | MEDIUM |
| CHECK-07.2 | Environment-specific config uses IConfiguration/IOptions pattern | MEDIUM |
| CHECK-07.3 | Sensitive configuration uses secrets management | HIGH |
| CHECK-07.4 | Feature flags managed through configuration, not code branches | MEDIUM |

### RDIM-08: Runtime Internal Partitioning

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | Control plane, middleware, projection, and routing are separate concerns | HIGH |
| CHECK-08.2 | No circular dependencies within runtime submodules | HIGH |
| CHECK-08.3 | Runtime submodules interact through defined internal contracts | MEDIUM |

### RDIM-09: Sole Persist / Publish / Anchor Authority

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Runtime is the ONLY layer that persists aggregate state — no engine, system, or platform layer calls `SaveAsync`, `SaveChanges`, `IRepository.Save`, or any storage write | CRITICAL |
| CHECK-09.2 | Runtime injects `IAggregateStore` into `EngineContext` as a private dependency — engines access persistence only through `EngineContext.EmitEvents()` | CRITICAL |
| CHECK-09.3 | Runtime is the ONLY layer that publishes domain events to external consumers (Kafka, outbox, webhooks) — no engine or domain layer calls publish/produce | CRITICAL |
| CHECK-09.4 | Runtime is the ONLY layer that anchors events to WhyceChain — no engine creates `ChainBlock` entries or calls chain anchor services | CRITICAL |
| CHECK-09.5 | After engine execution, runtime collects `EngineContext.EmittedEvents` and processes them through the persist → publish → anchor pipeline | HIGH |
| CHECK-09.6 | No `SaveChanges()`, `DbContext`, `IRepository.Save()`, or SQL calls exist in `src/engines/` | CRITICAL |
| CHECK-09.7 | No `IEventBus.Publish()`, `IMediator.Publish()`, `IEventPublisher`, Kafka produce calls exist in `src/engines/` | CRITICAL |
| CHECK-09.8 | No direct infra calls (Redis, HTTP, gRPC, SMTP, cloud SDK) exist in `src/engines/` — all infra interaction is runtime's responsibility | CRITICAL |
| CHECK-09.9 | No engine invocation path exists that bypasses the RuntimeControlPlane middleware pipeline | CRITICAL |

### RDIM-10: Projection Separation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | Runtime projections are not exposed via API | CRITICAL |
| CHECK-10.2 | Runtime projections are not used as business read models | CRITICAL |
| CHECK-10.3 | Runtime projections support execution only | HIGH |

### RDIM-11: Policy + Guard Mandatory

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | Policy middleware is present in pipeline | CRITICAL |
| CHECK-11.2 | Guard middleware (pre + post) exists | CRITICAL |
| CHECK-11.3 | Execution is blocked if policy fails | CRITICAL |

### RDIM-12: End-to-End Execution Proof

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-12.1 | Runtime produces domain events after execution | CRITICAL |
| CHECK-12.2 | Runtime persists events before publish | CRITICAL |
| CHECK-12.3 | Runtime publishes events to Kafka | CRITICAL |
| CHECK-12.4 | Runtime triggers projections after publish | CRITICAL |
| CHECK-12.5 | Runtime returns response after full pipeline | CRITICAL |

### RDIM-13: Deterministic Execution Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-13.1 | ExecutionHash generated deterministically | CRITICAL |
| CHECK-13.2 | No non-deterministic branching in runtime | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: runtime
status: PASS | FAIL
score: {0-100}
scope: "Runtime layer compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: RDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "runtime"
approval: GRANTED | BLOCKED
blocking_violations: {count of CRITICAL/HIGH}
```

---

## SCORING

| Start Score | 100 |
|-------------|-----|
| CRITICAL violation | -10 per occurrence |
| HIGH violation | -5 per occurrence |
| MEDIUM violation | -2 per occurrence |
| LOW violation | -1 per occurrence |
| Floor | 0 |
| Pass threshold | >= 80 |

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-R-CTX-01**: Confirm CommandContext.PolicyDecisionHash declared "{ get; set; }" (not init). PolicyMiddleware uses in-place mutation, not "with { ... }".
- **CHECK-R-ORDER-01**: Inspect RuntimeControlPlaneBuilder middleware list against locked 11-stage order. Idempotency MUST appear AFTER PolicyMiddleware and AuthorizationGuardMiddleware.
- **CHECK-R-UOW-01**: ExecuteEngineAsync wraps Append/Anchor/Outbox in unit-of-work or saga; otherwise S1.
- **CHECK-R-WORKFLOW-PIPE-01**: ExecuteWorkflowAsync explicitly invokes persist->chain->outbox after collecting workflow events.
- **CHECK-R-DOM-LEAK-01**: grep "Whycespace.Domain" / "*EventSchema" inside src/runtime/** outside dispatcher allowlist => S1.
- **CHECK-R-POLICY-PATH-01**: glob src/runtime/policies/**/*.cs nested by classification/context/domain => S2.

### CHECK: R-POLICY-FIRST-01
Verify policy executes before aggregate load.

### CHECK: R-WF-EVENTIFIED-01 (NEW 2026-04-07)
- FAIL if `src/runtime/workflow-state/` exists.
- FAIL if `src/shared/contracts/runtime/IWorkflowStateRepository.cs` or `IWorkflowStepObserver.cs` exists.
- FAIL if `RuntimeCommandDispatcher` constructor accepts `IWorkflowStateRepository`.
- FAIL if `WorkflowExecutionContext` exposes a `StepObserver` property.
- PASS requires the workflow execution lifecycle to be expressed exclusively as domain events emitted by `WorkflowLifecycleEventFactory` in `src/engines/T1M/lifecycle/` and projected by `WorkflowExecutionProjectionHandler` in `src/projections/orchestration-system/workflow/`.

### CHECK: R-WF-RESUME-01 (NEW 2026-04-07)
- ADVISORY: `WorkflowResumeCommand` returns a structured failure pending introduction of `IWorkflowExecutionReplayService`. Tracked in `claude/new-rules/`.

## TRACEABILITY REFERENCE — 2026-04-07

MAP: see claude/traceability/guard-traceability.map.md
- Each CHECK in this audit maps to a Guard Rule ID, Enforcement Point, and Evidence as defined in the master traceability map.
