# Structural Guard

## Purpose

Enforce repository partition boundaries and layer purity across the WBSM v3 architecture. No layer may reach into another layer's internal structure, and dependency direction must flow inward (platform > systems > runtime > engines > domain).

## Scope

All files under `src/`. Applies to every commit, PR, and CI pipeline run.

## Rules

1. **DOMAIN ISOLATION** — `src/domain/` has ZERO external dependencies. No NuGet packages, no infrastructure imports, no framework references. Domain references only `src/shared/` kernel primitives. No `using` directive may reference any namespace outside `Domain` or `Shared.Kernel`.

2. **SHARED PURITY** — `src/shared/` contains ONLY contracts, kernel primitives, and cross-cutting value types. It must NOT contain business logic, domain rules, workflow orchestration, or persistence concerns. Shared may not reference domain, engines, runtime, systems, or platform.

3. **ENGINE TIER COMPLIANCE** — `src/engines/` follows the T0U-T4A tiered topology. Engines import from `src/domain/` and `src/shared/` only. Engines NEVER define domain aggregates, entities, value objects, or domain events. Engines never reference other engines, runtime, systems, or platform.

4. **RUNTIME BOUNDARY** — `src/runtime/` is the control plane, middleware, and internal projection layer. Runtime may reference engines, domain, and shared. Runtime must NOT contain domain model definitions. Runtime must NOT contain direct persistence logic (only internal projection handlers). Runtime must NOT reference `src/projections/`.

5. **SYSTEMS COMPOSITION** — `src/systems/` is composition-only. Systems compose engines via runtime references. Systems must NOT contain execution logic, domain model definitions, or direct persistence. Systems may reference runtime and shared only.

6. **PLATFORM ENTRY** — `src/platform/` is the entry layer (API controllers, CLI, host configuration). Platform must NOT reference engines directly. Platform must NOT contain direct database access. Platform references systems and runtime only.

7. **DOMAIN PROJECTIONS LAYER** — `src/projections/` is the domain projection layer (CQRS read models / query layer). Projections may reference ONLY `src/shared/` and `infrastructure/` adapters. Projections must NOT reference `src/domain/`, `src/runtime/`, `src/engines/`, `src/systems/`, or `src/platform/`. All domain projections are event-driven only (Kafka/event fabric). Redis/read-store writes originate ONLY from this layer.

8. **INFRASTRUCTURE ADAPTERS** — `infrastructure/` contains only adapter implementations (repositories, messaging, external service clients). Infrastructure must NOT contain business logic or domain rules. Infrastructure implements interfaces defined in domain or shared.

9. **DEPENDENCY DIRECTION** — Dependencies flow strictly: `platform > systems > runtime > engines > domain < shared`. `src/projections/` is an isolated read-side layer referencing only `shared` and `infrastructure`. No reverse dependency is permitted. No lateral dependency within the same layer (e.g., engine-to-engine). Runtime and projections are mutually isolated — neither may reference the other.

10. **NAMESPACE ALIGNMENT** — File namespace must match its physical folder path. A file in `src/domain/economic/capital/vault/` must have namespace `Domain.Economic.Capital.Vault.*`. No namespace aliasing to circumvent layer boundaries.

11. **NO TRANSITIVE LEAKAGE** — Public types in inner layers must not expose types from outer layers in their signatures. Domain types must not reference engine or runtime types even transitively.

12. **TEST LAYER MUST MIRROR DOMAIN** — The test project structure must mirror the domain structure. For each BC at `src/domain/{system}/{context}/{domain}/`, a corresponding test folder must exist at `tests/{system}/{context}/{domain}/`. Test classes must follow the naming pattern `{ClassName}Tests`. Missing test mirrors for D2-level BCs are a structural violation.

13. **NO BUSINESS LOGIC OUTSIDE DOMAIN** — Business rules, domain invariants, and business decision logic must reside exclusively in `src/domain/`. No business logic in engines (engines execute domain operations, not define rules), runtime (runtime orchestrates, not decides), systems (systems compose, not compute), platform (platform routes, not validates), shared (shared carries types, not logic), infrastructure (infrastructure adapts, not reasons), or projections (projections flatten and denormalize, not decide).

14. **EVENT STORE IS SOURCE OF TRUTH** — The event store is the canonical source of truth for all domain state. Aggregate state is derived by replaying events. No alternative persistence mechanism may serve as the authoritative state source. Read models, caches, and projections are derived views — never authoritative. If the event store and a projection disagree, the event store is correct.

15. **DUAL PROJECTION MODEL** — WBSM v3 enforces two isolated projection layers:
    - `src/runtime/projection/` — internal execution support (workflow state, idempotency tracking, policy linking). NOT exposed externally. May be synchronous.
    - `src/projections/` — domain read models (CQRS query layer). Event-driven ONLY. The sole query source for APIs. Owns Redis/read-store writes.
    Both layers are mandatory, isolated, and non-overlapping. Any cross-reference between them is a CRITICAL violation.

---

## WBSM v3 GLOBAL ENFORCEMENT

### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

## Check Procedure

1. Parse all `using` directives and `import` statements across `src/`.
2. Build a dependency graph mapping each file to its referenced namespaces.
3. For each layer, verify that all references point only to permitted layers per Rule 9.
4. Verify no domain file references any namespace outside `Domain.*` or `Shared.Kernel.*`.
5. Verify no shared file references any namespace outside `Shared.*`.
6. Verify no engine file references `Engine.*`, `Runtime.*`, `Systems.*`, or `Platform.*`.
7. Verify all infrastructure files implement interfaces from domain or shared (not define new domain types).
8. Verify namespace-to-folder alignment for all files.
9. Scan for `DbContext`, `HttpClient`, `IConfiguration`, or framework types in domain layer.
10. Scan for aggregate/entity/value-object class definitions outside `src/domain/`.
11. Verify `src/projections/` exists and contains domain-aligned projection modules.
12. Verify `src/projections/` .csproj references ONLY `src/shared/` (no Domain, Runtime, Engines).
13. Verify no `src/projections/` file references Runtime, Domain, or Engines namespaces.
14. Verify no `src/runtime/projection/` file references Projections namespace.
15. Verify `src/runtime/projection/` exists and contains execution-support projections only.

## Pass Criteria

- All dependency arrows flow in the permitted direction.
- Zero cross-layer import violations detected.
- Zero domain model definitions found outside `src/domain/`.
- Zero business logic found in `src/shared/` or `infrastructure/`.
- All namespaces align with physical folder structure.
- `src/projections/` exists with domain-aligned projection modules.
- `src/runtime/projection/` exists with execution-support projections.
- Zero cross-references between runtime projections and domain projections.
- Domain projections reference only `src/shared/`.

## Fail Criteria

- **ANY** cross-layer import violation (e.g., domain referencing an engine).
- Domain file importing a NuGet package or framework namespace.
- Shared file containing business logic (conditional domain rules, workflow steps).
- Engine defining a domain aggregate, entity, or value object.
- Platform file directly referencing an engine namespace.
- Platform file containing direct database access (`DbContext`, raw SQL).
- Infrastructure file containing domain business rules.
- Namespace misalignment with folder path.
- `src/projections/` referencing domain, runtime, or engines.
- `src/runtime/projection/` referencing `src/projections/`.
- `src/projections/` directory missing.
- `src/runtime/projection/` directory missing.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Domain imports external dependency | `using Microsoft.EntityFrameworkCore;` in domain |
| **S0 — CRITICAL** | Reverse dependency direction | Engine importing from runtime |
| **S1 — HIGH** | Domain model defined outside domain | Aggregate class in `src/engines/` |
| **S1 — HIGH** | Business logic in shared or infrastructure | If/else domain rules in `src/shared/` |
| **S0 — CRITICAL** | Domain projections reference runtime/domain/engines | `using Whycespace.Runtime;` in `src/projections/` |
| **S0 — CRITICAL** | Runtime projections reference domain projections | `using Whycespace.Projections;` in `src/runtime/projection/` |
| **S0 — CRITICAL** | Missing projection layer | `src/projections/` or `src/runtime/projection/` does not exist |
| **S2 — MEDIUM** | Platform referencing engine directly | `using Engines.T2E.*;` in platform controller |
| **S2 — MEDIUM** | Namespace misalignment | File path and namespace disagree |
| **S3 — LOW** | Unnecessary transitive reference | Shared referencing a type it does not use |

## Enforcement Action

- **S0**: Block merge. Fail CI. Require immediate remediation.
- **S1**: Block merge. Fail CI. Must be resolved before next review cycle.
- **S2**: Warn in CI. Must be resolved within current sprint.
- **S3**: Advisory. Log for tech debt tracking.

All violations produce a structured report:
```
STRUCTURAL_GUARD_VIOLATION:
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct behavior>
  actual: <detected behavior>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **STR-AUTH-01**: Every controller action with HTTP verb POST/PUT/PATCH/DELETE MUST be protected by [Authorize] (controller or action) OR by global authentication middleware registered before MVC routing. A structural scan MUST fail the build for any unprotected mutating endpoint.
- **STR-HEALTH-01**: Health-check adapter implementations live in src/infrastructure/health/. Contracts (IHealthCheck, HealthCheckResult) live in src/shared/contracts/infrastructure/health/. HealthAggregator lives in infrastructure and is exposed only via platform API.
- **STR-OBS-01**: Observability middleware (metrics/logging/tracing) lives ONLY in src/runtime/observability/. MUST NOT contain business logic or domain references.

## NEW RULES INTEGRATED — 2026-04-08 (guard-registry drift)

- **STR-GUARD-REGISTRY-01** (S2): CLAUDE.md $1a guard list drifts from on-disk `claude/guards/*.guard.md`.
  The canonical loading semantics MUST be "load every `*.guard.md` under `/claude/guards/`" rather than
  an explicit enumeration, so the set self-heals against future guard additions. Any explicit
  enumeration in CLAUDE.md $1a MUST match the on-disk set exactly, or be replaced with the glob
  directive. Phase-1 authored guards (clean-code, composition-loader, dependency-graph, determinism,
  deterministic-id, hash-determinism, platform, program-composition, replay-determinism, runtime-order)
  are canonical. Source:
  `claude/new-rules/_archives/20260408-090519-guards.md`.
