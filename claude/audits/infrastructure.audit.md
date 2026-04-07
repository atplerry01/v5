# INFRASTRUCTURE AUDIT — WBSM v3

```
AUDIT ID:       INFRA-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the infrastructure layer to ensure strict compliance with WBSM v3 adapter-only architecture. Infrastructure is exclusively for adapter implementations — repository implementations, external service adapters, and configuration management. It must contain ZERO business logic and ZERO domain leakage.

This audit MUST detect:

* Business logic in infrastructure
* Domain model definitions in infrastructure
* Domain leakage (aggregate behavior replicated in adapters)
* Missing repository interface implementations
* Non-adapter artifacts in infrastructure
* Configuration management violations

---

## SCOPE

```
infrastructure/              -> all infrastructure adapters
infrastructure/persistence/  -> database adapters (Postgres, etc.)
infrastructure/messaging/    -> messaging adapters (Kafka, etc.)
infrastructure/external/     -> external service adapters (HTTP, gRPC, etc.)
infrastructure/config/       -> configuration management
infrastructure/caching/      -> cache adapters (Redis, etc.)
```

Excluded: `src/domain/`, `src/engines/`, `src/runtime/`, `src/systems/`, `src/platform/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Business logic in infrastructure, domain model definition, domain leakage | Blocks deployment |
| HIGH | Missing repository implementation, non-adapter artifact, missing interface | Must fix before merge |
| MEDIUM | Configuration hardcoding, non-standard adapter pattern | Fix within sprint |
| LOW | Missing logging in adapter, documentation gap | Fix at convenience |

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

### IDIM-01: Adapter-Only Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | No business rules or conditional domain logic in infrastructure code | CRITICAL |
| CHECK-01.2 | No `if/else/switch` statements that make domain decisions | CRITICAL |
| CHECK-01.3 | No domain calculations or computations in adapters | CRITICAL |
| CHECK-01.4 | Infrastructure performs only translation, mapping, and I/O | HIGH |
| CHECK-01.5 | No domain event creation in infrastructure code | CRITICAL |

### IDIM-02: Repository Implementations

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Each domain repository interface has a corresponding infrastructure implementation | HIGH |
| CHECK-02.2 | Repository implementations use domain interfaces from shared layer | HIGH |
| CHECK-02.3 | Repository implementations handle persistence concerns only (SQL, ORM mapping) | HIGH |
| CHECK-02.4 | Repository implementations do not add behavior beyond CRUD + query | CRITICAL |
| CHECK-02.5 | Repository implementations properly map between domain entities and persistence models | HIGH |

### IDIM-03: External Service Adapters

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | External service adapters implement shared-layer interfaces | HIGH |
| CHECK-03.2 | External service adapters handle network concerns (retry, timeout, circuit breaker) | MEDIUM |
| CHECK-03.3 | External service adapters translate external DTOs to domain-compatible types | HIGH |
| CHECK-03.4 | External service adapters do not expose external API details to domain | HIGH |
| CHECK-03.5 | No domain logic triggered by external service responses | CRITICAL |

### IDIM-04: Configuration Management

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Connection strings externalized (not hardcoded) | HIGH |
| CHECK-04.2 | Sensitive credentials use secrets management (not plain text) | CRITICAL |
| CHECK-04.3 | Configuration follows IOptions/IConfiguration pattern | MEDIUM |
| CHECK-04.4 | Environment-specific overrides supported | MEDIUM |
| CHECK-04.5 | No configuration values embedded in adapter logic | HIGH |

### IDIM-05: No Domain Leakage

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | No domain aggregate classes defined in infrastructure | CRITICAL |
| CHECK-05.2 | No domain entity classes defined in infrastructure | CRITICAL |
| CHECK-05.3 | No domain value object classes defined in infrastructure | CRITICAL |
| CHECK-05.4 | No domain service classes defined in infrastructure | CRITICAL |
| CHECK-05.5 | Persistence models (ORM entities) are separate from domain models | HIGH |
| CHECK-05.6 | Mapping layer exists between persistence models and domain models | HIGH |

### IDIM-06: Messaging Adapters

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | Kafka producer/consumer implementations are in infrastructure only | HIGH |
| CHECK-06.2 | Messaging adapters implement shared-layer interfaces | HIGH |
| CHECK-06.3 | Message serialization/deserialization is adapter-scoped | HIGH |
| CHECK-06.4 | No message transformation that alters domain semantics | CRITICAL |

### IDIM-07: Caching Adapters

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Cache implementations are in infrastructure only | HIGH |
| CHECK-07.2 | Cache adapters implement shared-layer interfaces | HIGH |
| CHECK-07.3 | Cache invalidation strategy is infrastructure-scoped (not domain logic) | HIGH |
| CHECK-07.4 | No domain state derived solely from cache (cache is optimization, not source of truth) | CRITICAL |

### IDIM-08: Database Migration Management

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | Database migrations are in infrastructure layer | HIGH |
| CHECK-08.2 | Migrations are versioned and sequential | HIGH |
| CHECK-08.3 | No migration contains business logic | CRITICAL |
| CHECK-08.4 | Migration rollback strategy defined | MEDIUM |

### IDIM-09: Projection Persistence Boundary

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Projection persistence handled via infrastructure adapters only | HIGH |
| CHECK-09.2 | No domain logic embedded in projection storage adapters | CRITICAL |

### IDIM-10: E2E Execution Integrity (Phase 1 — E2EDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | Infrastructure adapters participate in Platform → Systems → Runtime → Engines → Domain path only as persistence/messaging endpoints | CRITICAL |
| CHECK-10.2 | No infrastructure adapter bypasses runtime to call engines or domain directly | CRITICAL |
| CHECK-10.3 | Infrastructure adapters are invoked only by runtime (not by systems, platform, or engines) | CRITICAL |
| CHECK-10.4 | End-to-end execution produces persistence artifacts through infrastructure adapters | HIGH |
| CHECK-10.5 | Infrastructure adapters support event persistence before publication | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: infrastructure
status: PASS | FAIL
score: {0-100}
scope: "Infrastructure layer compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: IDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "infrastructure"
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

- **CHECK-INFRA-HEALTH-01** (S-HEALTH-01): Verify health-check placement per STR-HEALTH-01.
- **CHECK-INFRA-OBS-01** (S-OBSERVABILITY-01): Verify observability middleware placement per STR-OBS-01.
- **CHECK-INFRA-CLOCK-01** (S-CLOCK-01): Verify IClock present in shared kernel and consumed by all timestamp generators outside domain aggregates.
- **CHECK-INFRA-TOPICS-01**: Bootstrap MUST apply event-store + outbox + chain migrations to event store DB (not just event-store migrations). Missing migration sets in initdb.d / bootstrap = S2.
- **CHECK-INFRA-PLACEHOLDER-01**: Every in-memory repository in production composition is marked as placeholder AND has corresponding scripts/migrations/*.sql ready.
