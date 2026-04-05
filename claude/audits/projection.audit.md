# PROJECTION AUDIT — WBSM v3 (Dual Projection Architecture)

```
AUDIT ID:       PROJECTION-AUDIT-v3
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
SUPERSEDES:     PROJECTION-AUDIT-v1 (REV 1)
```

## PURPOSE

Audit all projections to ensure strict compliance with the WBSM v3 Dual Projection Architecture. Two projection layers exist:

1. **Runtime Projections** (`src/runtime/projection/`) — internal execution support ONLY
2. **Domain Projections** (`src/projections/`) — business-facing read models / CQRS query layer

This audit MUST detect:

* Cross-layer dependency violations between runtime and domain projections
* Projections that issue commands
* Projections that consume commands (instead of events)
* Projections placed outside their designated layer
* Write operations in projection handlers
* Projections that violate eventual consistency model
* Missing materialized view compliance
* Runtime projections exposed via API
* Domain projections invoked synchronously
* Redis writes from wrong layer

---

## SCOPE

```
src/runtime/projection/   -> runtime projection handlers (execution support ONLY)
src/projections/          -> domain projection handlers (read models / CQRS query layer)
infrastructure/           -> projection persistence adapters
```

Excluded: `src/domain/`, `src/engines/`, `src/systems/`, `src/platform/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Cross-layer violation, command dispatch, aggregate mutation, wrong-layer exposure | Blocks deployment |
| HIGH | Wrong-layer placement, command consumption, missing idempotency, synchronous domain projection | Must fix before merge |
| MEDIUM | Missing rebuild capability, non-standard naming, missing context fields | Fix within sprint |
| LOW | Missing metrics, documentation gap | Fix at convenience |

---

## AUDIT DIMENSIONS

### PJDIM-01: Read-Only Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | Projection handlers contain no command dispatch calls | CRITICAL |
| CHECK-01.2 | Projection handlers contain no aggregate method invocations | CRITICAL |
| CHECK-01.3 | Projection handlers contain no domain service calls | CRITICAL |
| CHECK-01.4 | Projection handlers perform INSERT/UPDATE on read models only | HIGH |
| CHECK-01.5 | Projection handlers have no side effects beyond read model updates | HIGH |

### PJDIM-02: Event Consumption Only

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Projections are triggered by domain events only | CRITICAL |
| CHECK-02.2 | Projections do not subscribe to command topics | CRITICAL |
| CHECK-02.3 | Projection event handlers accept event types (not command types) | HIGH |
| CHECK-02.4 | Projections handle event ordering correctly (sequence/version) | HIGH |
| CHECK-02.5 | Projections handle out-of-order events gracefully | MEDIUM |

### PJDIM-03: Layer Placement (Dual Model)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | Runtime projection handlers are in `src/runtime/projection/` ONLY | CRITICAL |
| CHECK-03.2 | Domain projection handlers are in `src/projections/` ONLY | CRITICAL |
| CHECK-03.3 | No projection logic in `src/domain/` | CRITICAL |
| CHECK-03.4 | No projection logic in `src/engines/` | CRITICAL |
| CHECK-03.5 | No projection logic in `src/systems/` | HIGH |
| CHECK-03.6 | No projection logic in `src/platform/` | HIGH |
| CHECK-03.7 | Projection persistence adapters are in `infrastructure/` | HIGH |

### PJDIM-04: Eventual Consistency

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Projections accept eventual consistency (no synchronous read-after-write guarantees) | HIGH |
| CHECK-04.2 | Read models have version/sequence tracking for consistency monitoring | HIGH |
| CHECK-04.3 | Stale read detection mechanism available | MEDIUM |
| CHECK-04.4 | Projection lag is measurable and observable | MEDIUM |

### PJDIM-05: Materialized View Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | Each projection produces a well-defined read model schema | HIGH |
| CHECK-05.2 | Read model schemas are optimized for query patterns (denormalized where appropriate) | MEDIUM |
| CHECK-05.3 | Read models are rebuildable from event stream | HIGH |
| CHECK-05.4 | Read model tables/collections are separate from write model storage | CRITICAL |
| CHECK-05.5 | Read models have appropriate indexes for query performance | LOW |

### PJDIM-06: Idempotency

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | Projection handlers are idempotent (reprocessing same event produces same result) | HIGH |
| CHECK-06.2 | Duplicate event detection in projection handlers | HIGH |
| CHECK-06.3 | Projection checkpoint/offset tracking prevents reprocessing | HIGH |

### PJDIM-07: Rebuild Capability

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Projections support full rebuild from event store | HIGH |
| CHECK-07.2 | Rebuild does not affect live read traffic (blue/green or shadow) | MEDIUM |
| CHECK-07.3 | Rebuild progress is trackable | LOW |
| CHECK-07.4 | Rebuild can target a specific projection without affecting others | MEDIUM |

### PJDIM-08: Projection Isolation (S24 — NEW)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | `src/runtime/` does NOT reference `src/projections/` | CRITICAL |
| CHECK-08.2 | `src/projections/` does NOT reference `src/runtime/` | CRITICAL |
| CHECK-08.3 | `src/projections/` does NOT reference `src/domain/` | CRITICAL |
| CHECK-08.4 | `src/projections/` does NOT reference `src/engines/` | CRITICAL |
| CHECK-08.5 | `src/projections/` .csproj references ONLY `src/shared/` | CRITICAL |
| CHECK-08.6 | No `using` directive in `src/projections/` references Runtime, Domain, or Engines namespaces | CRITICAL |
| CHECK-08.7 | No `using` directive in `src/runtime/projection/` references Projections namespace | CRITICAL |

### PJDIM-09: Event-Driven Enforcement (Domain Projections)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | All `src/projections/` handlers consume events via Kafka/event fabric | HIGH |
| CHECK-09.2 | No direct method invocation from runtime into domain projections | CRITICAL |
| CHECK-09.3 | Domain projections are NOT invoked synchronously | HIGH |

### PJDIM-10: Storage Ownership

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | Redis/read-store writes originate ONLY from `src/projections/` | HIGH |
| CHECK-10.2 | Runtime projections do NOT write to Redis | HIGH |
| CHECK-10.3 | No shared storage between runtime and domain projections | HIGH |

### PJDIM-11: Exposure Rules

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | Runtime projections are NOT exposed via API endpoints | CRITICAL |
| CHECK-11.2 | `src/projections/` is the ONLY query source for external consumers | HIGH |
| CHECK-11.3 | No API controller directly queries `src/runtime/projection/` | CRITICAL |

### PJDIM-12: Context Field Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-12.1 | Projection handlers include CorrelationId in event processing context | MEDIUM |
| CHECK-12.2 | Projection handlers include EventId in event processing context | MEDIUM |
| CHECK-12.3 | Projection handlers include IdempotencyKey in event processing context | MEDIUM |

### PJDIM-13: Event-First Architecture (Phase 1 — EFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-13.1 | All projection state changes originate from domain events only | CRITICAL |
| CHECK-13.2 | No direct state mutation outside event handlers in projections | CRITICAL |
| CHECK-13.3 | Events are persisted before projection consumption | CRITICAL |
| CHECK-13.4 | Projections are driven by events only — no polling, no direct invocation | CRITICAL |

### PJDIM-14: Phase 1 Execution Visibility

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-14.1 | Sandbox projections exist and update correctly | CRITICAL |
| CHECK-14.2 | Todo projections exist and update correctly | CRITICAL |
| CHECK-14.3 | Projections reflect lifecycle transitions | HIGH |
| CHECK-14.4 | Projections reflect workflow steps | HIGH |

---

## OUTPUT FORMAT

```yaml
audit: projection
status: PASS | FAIL
score: {0-100}
scope: "Dual Projection Architecture compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: PJDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    layer: runtime_projection | domain_projection
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "projection"
approval: GRANTED | BLOCKED
blocking_violations: {count of CRITICAL/HIGH}
```

---

## CANONICAL STATEMENT

> `src/projections/` is a canonical top-level domain-aligned read model layer and MUST be treated as part of system architecture, not auxiliary infrastructure.

---

## CANONICAL RULES (S1.5)

| Rule | Description | Severity |
|------|-------------|----------|
| P1 | `src/projections/` is the canonical domain projection layer — ALLOW all projection handlers here | CRITICAL |
| P2 | No domain writes (aggregate mutations) inside projections — read model updates ONLY | CRITICAL |
| P3 | `src/runtime/projection/` must NOT be exposed outside runtime (no API, no systems, no platform references) | CRITICAL |
| P4 | Projection naming must be domain-aligned: `projection:{classification}.{context}.{domain}:{key}` | MEDIUM |

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
