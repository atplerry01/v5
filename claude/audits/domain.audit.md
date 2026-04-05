# DOMAIN AUDIT — WBSM v3

```
AUDIT ID:       DOMAIN-AUDIT-v2
REVISION:       REV 4
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the domain layer to ensure strict compliance with WBSM v3 domain topology, DDD conventions, and zero external dependency rules. The domain layer is the pure heart of the system — it must contain only aggregates, entities, value objects, events, errors, services, and specifications with absolutely no infrastructure or framework dependencies.

This audit MUST detect:

* CLASSIFICATION > CONTEXT > DOMAIN topology violations
* Incomplete aggregate structures (missing DDD folders)
* External dependencies in domain code
* DDD naming convention violations
* Mutable value objects
* Non-past-tense event names
* Activation level mismatches

---

## SCOPE

```
src/domain/                    -> all domain bounded contexts
src/domain/{classification}/   -> classification groupings
src/domain/{class}/{context}/  -> context groupings
src/domain/{class}/{ctx}/{bc}/ -> individual bounded contexts
```

Excluded: all other layers, `bin/`, `obj/`, `.git/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | External dependency in domain, flat topology, aggregate corruption, lifecycle/event integrity violation | Blocks deployment |
| HIGH | Missing DDD folder, non-past-tense event, mutable value object | Must fix before merge |
| MEDIUM | Naming convention violation, missing specification | Fix within sprint |
| LOW | Missing .gitkeep in empty folder, cosmetic issue | Fix at convenience |

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

### DDIM-01: CLASSIFICATION > CONTEXT > DOMAIN Topology

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | Every BC follows three-level nesting: `domain/{classification}/{context}/{domain}/` | CRITICAL |
| CHECK-01.2 | No BC exists directly under `domain/` (flat structure) | CRITICAL |
| CHECK-01.3 | No BC exists at two-level nesting `domain/{classification}/{bc}/` without context | CRITICAL |
| CHECK-01.4 | Classification folders match canonical registry (e.g., clusters, economic, governance, identity, humancapital, document, etc.) | HIGH |
| CHECK-01.5 | Context folders are semantically meaningful groupings within classification | MEDIUM |

### DDIM-02: Aggregate Completeness

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Each BC has an `aggregate/` folder | CRITICAL |
| CHECK-02.2 | Each BC has an `entity/` folder | HIGH |
| CHECK-02.3 | Each BC has a `value-object/` folder | HIGH |
| CHECK-02.4 | Each BC has an `event/` folder | CRITICAL |
| CHECK-02.5 | Each BC has an `error/` folder | HIGH |
| CHECK-02.6 | Each BC has a `service/` folder | HIGH |
| CHECK-02.7 | Each BC has a `specification/` folder | HIGH |

### DDIM-03: Zero External Dependencies

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | No `using` statements referencing `System.Data`, `System.Net`, `System.IO` (infrastructure) | CRITICAL |
| CHECK-03.2 | No NuGet package references in domain projects (except core .NET) | CRITICAL |
| CHECK-03.3 | No references to `Microsoft.EntityFrameworkCore` or any ORM | CRITICAL |
| CHECK-03.4 | No references to `Confluent.Kafka` or any messaging library | CRITICAL |
| CHECK-03.5 | No references to `Microsoft.AspNetCore` or any web framework | CRITICAL |
| CHECK-03.6 | No references to engines, runtime, systems, platform, or infrastructure namespaces | CRITICAL |

### DDIM-04: DDD Naming Conventions

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Aggregate root class name matches the domain concept (e.g., `Cluster`, `Vault`, `Proposal`) | MEDIUM |
| CHECK-04.2 | Entity class names describe subordinate domain concepts | MEDIUM |
| CHECK-04.3 | Value object class names describe immutable domain attributes | MEDIUM |
| CHECK-04.4 | Service class names end with `Service` | MEDIUM |
| CHECK-04.5 | Specification class names end with `Spec` or `Specification` | MEDIUM |

### DDIM-05: Domain Event Past-Tense Naming

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | All domain event classes use past-tense naming (e.g., `ClusterCreatedEvent`, not `CreateClusterEvent`) | HIGH |
| CHECK-05.2 | Event class names end with `Event` suffix | HIGH |
| CHECK-05.3 | Event names describe what happened, not what should happen | HIGH |

### DDIM-06: Value Object Immutability

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | Value objects have no public setters | CRITICAL |
| CHECK-06.2 | Value object properties are `init`-only or `readonly` | HIGH |
| CHECK-06.3 | Value objects implement structural equality (not reference equality) | HIGH |
| CHECK-06.4 | Value objects have no side-effecting methods | MEDIUM |

### DDIM-07: Aggregate Invariant Protection

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Aggregate constructors enforce invariants (validation in constructor) | HIGH |
| CHECK-07.2 | Aggregate state changes go through methods (not public setters) | CRITICAL |
| CHECK-07.3 | Aggregates raise domain events for state changes | HIGH |
| CHECK-07.4 | Aggregates do not expose mutable collections | HIGH |

### DDIM-08: Activation Level Compliance (D0/D1/D2)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | D0 BCs have folder structure + .gitkeep files only | LOW |
| CHECK-08.2 | D1 BCs have at minimum: aggregate root, at least one event, at least one error | HIGH |
| CHECK-08.3 | D2 BCs have full DDD artifacts: aggregate, entities, value objects, events, errors, services, specifications | HIGH |
| CHECK-08.4 | No BC claims D2 activation without engine wiring and runtime integration | HIGH |
| CHECK-08.5 | Activation level matches canonical registry declaration | CRITICAL |

### DDIM-09: Shared Value Objects (_shared)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | `_shared/value-object/` exists at classification level for cross-context value objects | MEDIUM |
| CHECK-09.2 | Shared value objects are truly shared (referenced by multiple contexts) | MEDIUM |
| CHECK-09.3 | Shared value objects follow same immutability rules as BC value objects | HIGH |

### DDIM-10: Projection Isolation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | No projection logic exists in `src/domain/` | CRITICAL |
| CHECK-10.2 | No dependency from domain to `src/projections/` | CRITICAL |

### DDIM-11: Determinism Enforcement (Phase 1 — DETDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | No `DateTime.UtcNow` or non-deterministic time usage in domain code | CRITICAL |
| CHECK-11.2 | No `Guid.NewGuid()` outside deterministic helper in domain code | CRITICAL |
| CHECK-11.3 | IDs generated via `DeterministicIdHelper` only | CRITICAL |
| CHECK-11.4 | Event ordering deterministic per aggregate | HIGH |
| CHECK-11.5 | Replay produces identical results from same event stream | HIGH |

### DDIM-12: Event-First Architecture (Phase 1 — EFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-12.1 | All state changes originate from domain events | CRITICAL |
| CHECK-12.2 | No direct state mutation outside aggregates | CRITICAL |
| CHECK-12.3 | Events are the sole record of state transitions in domain | CRITICAL |
| CHECK-12.4 | Domain models produce events — never consume external state directly | CRITICAL |

### DDIM-13: Lifecycle + Workflow Validation (Phase 1 — LWFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-13.1 | At least one lifecycle process implemented (created → active → completed) | CRITICAL |
| CHECK-13.2 | Lifecycle transitions enforce invariants via aggregate methods | CRITICAL |
| CHECK-13.3 | Lifecycle state transitions produce corresponding domain events | CRITICAL |
| CHECK-13.4 | Lifecycle states are modeled as value objects or enums in domain | HIGH |
| CHECK-13.5 | Invalid lifecycle transitions are rejected by aggregate invariant guards | HIGH |

### DDIM-14: Sandbox/Todo Mandatory (Phase 1 — SBDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-14.1 | Sandbox bounded context exists and contains executable domain artifacts | CRITICAL |
| CHECK-14.2 | Todo bounded context exists and contains executable domain artifacts | CRITICAL |
| CHECK-14.3 | Both demonstrate full vertical slice at domain layer (aggregate, events, errors, value objects) | CRITICAL |

### DDIM-15: Lifecycle + Event Integrity (Phase 1 — LEIDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-15.1 | Aggregates enforce lifecycle transitions via explicit methods (no arbitrary state jumps) | CRITICAL |
| CHECK-15.2 | Invalid lifecycle transitions are blocked by aggregate invariant guards | CRITICAL |
| CHECK-15.3 | All aggregate state changes emit corresponding domain events — no silent mutations | CRITICAL |
| CHECK-15.4 | Emitted events are deterministic (no non-deterministic IDs, timestamps, or random values) and structurally complete (all required fields populated) | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: domain
status: PASS | FAIL
score: {0-100}
scope: "Domain layer compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: DDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "domain"
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
