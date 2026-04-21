# Section Checklist (18 ordered sections)

The canonical structure every vertical bounded system (EX) must implement, derived from `claude/project-topics/v2b/phase-2b.md` lines 106–349 (structural vertical) and verified against the economic vertical's actual implementation.

Sections must be implemented in **dependency order**. Each section's definition-of-done is the gate to the next.

| # | Section | Definition of Done | Economic exemplar |
|---|---|---|---|
| 1 | System Definition & Mapping | Canonical scope, boundaries, terminology, naming/route/topic normalization documented in the system's README | [src/domain/economic-system/README.md](../../../src/domain/economic-system/README.md) |
| 2 | Core Model | Aggregates, entities, value objects, state model defined for every BC | [src/domain/economic-system/capital/](../../../src/domain/economic-system/capital/) |
| 3 | Identity & Reference | Deterministic IDs, linkage rules, immutable vs mutable fields | [src/domain/economic-system/capital/account/value-object/AccountId.cs](../../../src/domain/economic-system/capital/account/value-object/AccountId.cs) |
| 4 | Domain Implementation | Per-domain concrete builds (one folder per BC under classification/context/) | [src/domain/economic-system/](../../../src/domain/economic-system/) |
| 5 | Aggregate & Domain Design | Commands, events, queries, invariants, errors per BC | [src/domain/economic-system/capital/account/aggregate/CapitalAccountAggregate.cs](../../../src/domain/economic-system/capital/account/aggregate/CapitalAccountAggregate.cs) |
| 6 | Business Invariants (incl. Cross-System Invariants) | Uniqueness, integrity, state-transition rules enforced via specifications PLUS cross-system invariants declared under `src/domain/{cls}-system/invariant/{policy-name}/` per [01-domain-skeleton.md](01-domain-skeleton.md) § Cross-System Invariants | [src/domain/economic-system/capital/account/specification/](../../../src/domain/economic-system/capital/account/specification/) |
| 7 | Policy Integration | Policy IDs, packages, allow/deny coverage, simulation registered with WHYCEPOLICY | [infrastructure/policy/domain/economic/](../../../infrastructure/policy/domain/economic/) |
| 8 | Runtime & Middleware Integration | Routing, policy eval, authz, idempotency, guards, tracing all dispatch the new commands | [src/runtime/dispatcher/RuntimeCommandDispatcher.cs](../../../src/runtime/dispatcher/RuntimeCommandDispatcher.cs) |
| 9 | Application & Engine Services (incl. cross-system invariant enforcement) | Handlers, validators, coordinators per command — T1M (workflow steps) + T2E (single-shot handlers). Cross-system invariants enforced here per [02-engine-skeleton.md](02-engine-skeleton.md) § Cross-System Invariants (inline in T2E OR as `Evaluate{Concept}PolicyStep` in T1M). | [src/engines/T1M/domains/economic/](../../../src/engines/T1M/domains/economic/) and [src/engines/T2E/economic/](../../../src/engines/T2E/economic/) |
| 10 | Persistence & Event Sourcing | Streams, versioning, rehydration, replay-determinism — including value-object JsonConverter registration per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01` | [src/runtime/event-fabric/EventDeserializer.cs](../../../src/runtime/event-fabric/EventDeserializer.cs) |
| 11 | Messaging & Event Fabric | Topic map, outbox publish, retry, DLQ — naming compliance per `runtime.guard.md` | [src/platform/host/adapters/KafkaOutboxPublisher.cs](../../../src/platform/host/adapters/KafkaOutboxPublisher.cs) |
| 12 | Projections & Read Models | Reducers + handlers wired for every event; replay-validated | [src/projections/economic/](../../../src/projections/economic/) |
| 13 | API & Platform Exposure | Controllers, route mapping, contract validation — DTO naming per `domain.guard.md` DTO-R1..R4 | [src/platform/api/controllers/economic/](../../../src/platform/api/controllers/economic/) |
| 14 | Observability & Evidence | Traces, metrics, WhyceChain anchoring, alerts wired into the runtime pipeline | [src/runtime/observability/](../../../src/runtime/observability/) |
| 15 | Cross-System Integration (incl. Cross-System Invariant Layer) | Linkage to structural / identity / policy / chain / economic via shared events, PLUS cross-system invariants ensuring consistency across systems — domain-level policies spanning multiple BCs under `src/domain/{cls}-system/invariant/`. See [01-domain-skeleton.md](01-domain-skeleton.md), [02-engine-skeleton.md](02-engine-skeleton.md), [03-runtime-wiring.md](03-runtime-wiring.md) § 6b, [05-quality-gates.md](05-quality-gates.md) Gate 9, [06-infrastructure-contracts.md](06-infrastructure-contracts.md) § 06.10. | [src/shared/contracts/](../../../src/shared/contracts/) |
| 16 | Testing & Certification | Three-tier coverage: unit (domain), integration (command→event), e2e (API→projection) | [tests/unit/economic-system/](../../../tests/unit/economic-system/), [tests/integration/economic-system/](../../../tests/integration/economic-system/), [tests/e2e/economic/](../../../tests/e2e/economic/) |
| 17 | Resilience & Recovery | Interruption, retry, projection recovery, consistency under restart | [src/platform/host/adapters/KafkaOutboxPublisher.cs](../../../src/platform/host/adapters/KafkaOutboxPublisher.cs) (retry/DLQ pattern) |
| 18 | Documentation & Anti-Drift | READMEs, catalogs, guard/audit updates, completion evidence pack stored under `claude/audits/` | [claude/audits/](../../../claude/audits/) |

## Application order

1. **Sections 1–6** define the domain. **Cannot start section 7+ without these.**
2. **Sections 7–9** wire policy, runtime, engines. **Cannot ship without all three.**
3. **Sections 10–12** persist and project. **Required for replay safety + read models.**
4. **Section 13** exposes via API. **Cannot test e2e without it.**
5. **Sections 14–15** add observability + cross-system seams. **Required for production readiness.**
6. **Sections 16–17** validate. **Definition of "done".**
7. **Section 18** documents. **Required for promotion to D2 (Active).**

## D-level gates

| Level | Definition | Sections complete |
|---|---|---|
| **D0 (Scaffold)** | Folder structure with `.gitkeep` placeholders | 1 |
| **D1 (Partial)** | Some artifacts implemented but not all BCs wired | 1–6 |
| **D2 (Active)** | All sections done, all quality gates pass | 1–18 |

Per `domain.guard.md` rule 10, **a BC must not be consumed by engines unless at D2 level.**

## Cross-System Invariant Layer (cross-cutting concern)

The **Cross-System Invariant Layer** is a first-class concern that touches sections 6, 9, 15, and 16 but does not add a 19th section. It guarantees **cross-system truth — consistency between all systems** — alongside structural, business, content, economic, and operational truth. Full detail:

| Template file | Cross-system invariant content |
|---|---|
| [01-domain-skeleton.md](01-domain-skeleton.md) | § Cross-System Invariants — concept definition, folder layout `src/domain/{cls}-system/invariant/{policy-name}/`, naming, examples |
| [02-engine-skeleton.md](02-engine-skeleton.md) | § Cross-System Invariants — Domain Policy Layer, enforcement in T2E / T1M, fail-fast pattern |
| [03-runtime-wiring.md](03-runtime-wiring.md) | § 6b Cross-system domain invariants — composition-root registration via `AddDomain{Vertical}Invariants()` |
| [05-quality-gates.md](05-quality-gates.md) | § Gate 9 Cross-System Invariants — declaration, implementation, enforcement, purity, replay safety |
| [06-infrastructure-contracts.md](06-infrastructure-contracts.md) | § 06.10 Policy Integration — code policies vs OPA policies, externalization rules, replay-independent evaluation |
