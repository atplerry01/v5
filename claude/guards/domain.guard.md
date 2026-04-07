# Domain Guard

## Purpose

Enforce DDD structural and naming conventions across all bounded contexts in `src/domain/`. Every BC must follow the canonical super-classification system architecture with CLASSIFICATION > CONTEXT > DOMAIN topology, use proper DDD artifact types, and comply with domain activation levels (D0/D1/D2).

## Scope

All files and folders under `src/domain/`. Applies to every BC, aggregate, entity, value object, event, error, service, and specification. Evaluated at CI, code review, and architectural audit.

## Canonical Super-Classification Systems

The domain layer is organized into exactly **11 root systems** plus `shared-kernel`:

| System | Concern |
|--------|---------|
| `business-system` | Business logic: agreements, billing, documents, entitlements, execution, integration, inventory, localization, logistics, marketplace, notification, portfolio, resource, scheduler, subscription |
| `constitutional-system` | Policy, chain (immutable ledger) |
| `core-system` | Command/event/state framework primitives, financial control, reconciliation, temporal |
| `decision-system` | Governance, audit, compliance, risk |
| `economic-system` | Financial execution: asset, binding, capital, charge, distribution, enforcement, ledger, limit, payout, pricing, reserve, revenue, settlement, transaction, treasury, vault, wallet |
| `intelligence-system` | Analysis, simulation, forecasting, observability, geo, search, knowledge, estimation, planning |
| `operational-system` | Runtime operations: global incident response, sandbox |
| `orchestration-system` | Workflow: definition, execution, compensation, routing, stages |
| `structural-system` | Clusters (authority, classification, topology, lifecycle) and human capital |
| `trust-system` | Identity (13 sub-domains) and access (6 sub-domains: authorization, grant, permission, request, role, session) |
| `shared-kernel` | Kernel base classes, primitives (identity, money, time), jurisdiction, location, measurement |

### Forbidden Legacy Folders

These MUST NOT exist under `src/domain/`:
- `business/` (use `business-system/`)
- `constitutional/` (use `constitutional-system/`)
- `core-access-trust/`
- `chain/` (must be inside `constitutional-system/`)
- `identity-federation/` (must be inside `trust-system/identity/`)

## Rules

1. **CLASSIFICATION > CONTEXT > DOMAIN TOPOLOGY** — Every bounded context must follow the three-level hierarchy: `src/domain/{system}/{context}/{domain}/`. System is the super-classification (e.g., `economic-system`, `trust-system`). Context is the functional area within that system (e.g., `access`, `identity`, `capital`, `ledger`). Domain is the specific BC (e.g., `grant`, `credential`, `vault`, `accounting`). **No domain may sit directly under a system without a context layer.**

2. **MANDATORY ARTIFACT FOLDERS** — Each domain (BC) must contain exactly these subfolders: `aggregate/`, `entity/`, `error/`, `event/`, `service/`, `specification/`, `value-object/`. Missing folders indicate an incomplete BC. Placeholder `.gitkeep` files are acceptable for D0-level BCs.

3. **AGGREGATES ARE SOLE ENTRY POINTS** — Aggregate roots are the only public entry points into a BC. External code (engines, runtime) must interact with the BC through its aggregate(s). No external code may directly instantiate or call methods on entities or value objects without going through the aggregate.

4. **ENTITIES OWNED BY AGGREGATES** — Entities exist only within the boundary of their parent aggregate. Entity classes must not be independently instantiated or persisted outside their aggregate root. Entity identity is local to the aggregate.

5. **VALUE OBJECTS ARE IMMUTABLE** — All value objects must be immutable. Value object classes must have no public setters. All properties must be `{ get; }` or `{ get; init; }`. Value objects are compared by value, not reference. Value objects must override `Equals()` and `GetHashCode()` or use record types.

6. **DOMAIN EVENTS NAMED PAST-TENSE** — All domain event classes must be named in past tense, indicating something that has already happened. Pattern: `{Subject}{Action}Event` where Action is past-tense (e.g., `OrderCreatedEvent`, `PaymentProcessedEvent`, `VaultCreditedEvent`). Present or future tense is forbidden.

7. **ERRORS ARE DOMAIN-SPECIFIC** — Error/exception classes in `error/` must represent domain-specific failure states, not technical errors. Domain errors carry business meaning (e.g., `InsufficientBalanceError`, `ContractExpiredError`). No generic `Exception` subclasses in domain.

8. **SERVICES ARE STATELESS** — Domain services in `service/` must be stateless. They must not hold mutable instance state across calls. Services coordinate domain logic that does not belong to a single aggregate. Services receive all required data as method parameters.

9. **SPECIFICATIONS ARE PURE** — Specification classes in `specification/` must be pure functions: given the same input, always return the same boolean result. No side effects, no I/O, no database access. Specifications encapsulate domain rules as composable predicates.

10. **DOMAIN ACTIVATION LEVELS** — BCs are classified by activation level:
    - **D0 (Scaffold)**: Folder structure exists with `.gitkeep` placeholders. No implementation.
    - **D1 (Partial)**: Some artifacts implemented (e.g., aggregate + value objects). Not yet complete.
    - **D2 (Active)**: All mandatory artifacts implemented with business logic. Ready for engine consumption.
    A BC must not be consumed by engines unless at D2 level.

11. **SHARED VALUE OBJECTS PER CLASSIFICATION** — Each classification may have a `_shared/value-object/` folder for value objects shared across BCs within that classification (e.g., `economic-system/_shared/value-object/Rate.cs`). Shared value objects must not contain business logic — identity and type only.

12. **NO EXTERNAL DEPENDENCIES** — Domain code must not reference any external package, framework, or infrastructure concern. No ORM attributes, no serialization attributes (unless from shared kernel), no HTTP types, no logging frameworks. Pure domain only.

13. **NO CROSS-BC REFERENCES** — A BC must not directly reference types from another BC. Cross-BC communication is via domain events or shared kernel value objects only. No `using Whyce.Domain.EconomicSystem.Vault;` from within `Whyce.Domain.EconomicSystem.Ledger`.

14. **AGGREGATE NAMING** — All aggregate classes must be named `{DomainConcept}Aggregate` (e.g., `GrantAggregate`, `SessionAggregate`, `VaultAggregate`). Bare names like `Grant.cs` or `Session.cs` for aggregates are forbidden.

15. **EVENT NAMING** — All domain event classes must follow `{Subject}{PastTenseVerb}Event` (e.g., `GrantApprovedEvent`, `SessionStartedEvent`). Present or future tense is forbidden.

16. **NO PREFIX NOISE** — Domain folder names must NOT carry their parent context as a prefix. Use `grant/` not `access-grant/`. Use `request/` not `access-request/`.

17. **NO DUPLICATE DOMAINS** — The same domain concept must not exist in multiple systems or contexts. If `permission` exists under `trust-system/access/`, it must not also exist under `trust-system/identity/` unless the bounded contexts are semantically distinct and documented.

18. **CHAIN ISOLATION** — The `chain` domain (immutable ledger) must exist ONLY under `constitutional-system/chain/`. No other system may contain a `chain` domain folder.

19. **FEDERATION ISOLATION** — The `federation` domain must exist ONLY under `trust-system/identity/federation/`. No other system may contain a `federation` domain folder.

20. **NAMESPACE STANDARD** — All domain classes must use the namespace pattern: `Whyce.Domain.<System>.<Context>.<Domain>` (e.g., `Whyce.Domain.TrustSystem.Access.Grant`). System names use PascalCase without hyphens.

21. **SHARED KERNEL PURITY** — `shared-kernel/` must contain ONLY: base abstractions (AggregateRoot, Entity, ValueObject, DomainEvent, Specification), cross-domain primitives (identity, money, time), and cross-domain value objects (jurisdiction, location, measurement). NO domain aggregates, NO business logic, NO domain-specific services.

22. **NO POLICY LOGIC IN DOMAIN** — Domain aggregates, entities, services, and specifications must not evaluate authorization or policy decisions. Domain must not check actor roles, permissions, or policy satisfaction. Authorization is a runtime/policy concern that is resolved before the command reaches the domain. Domain enforces business invariants only.

23. **EVENT REQUIRED FOR STATE CHANGE** — Every aggregate state mutation must emit at least one domain event. No direct field assignment or property mutation without a corresponding event. The aggregate's state is the projection of its event history. Silent mutations break event sourcing and auditability.

24. **AGGREGATE VERSIONING REQUIRED** — All aggregates must support optimistic concurrency via a `Version` property. Each event application increments the version. Concurrent modifications are detected by version mismatch. Aggregates without versioning cannot participate in event-sourced persistence.

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

1. Verify only the 11 canonical systems + `shared-kernel` exist at root level. No legacy folders.
2. Enumerate all folders under `src/domain/` and verify three-level topology: `{system}/{context}/{domain}/`.
3. Verify NO domain sits directly under a system (must have context layer).
4. For each domain folder, verify presence of all seven mandatory subfolders.
5. Classify each BC's activation level (D0/D1/D2) based on artifact completeness.
6. Verify all aggregate classes use `{Name}Aggregate` naming and are `public`.
7. Verify entity classes are not independently instantiated outside their aggregate's namespace.
8. Verify all value object properties are readonly (`{ get; }` or `{ get; init; }` or `record` type).
9. Verify all event class names match the `{Subject}{PastTenseVerb}Event` pattern.
10. Verify error classes represent domain-specific states (not generic technical errors).
11. Verify service classes have no mutable instance fields.
12. Verify specification classes have no side effects (no I/O, no `async`, no mutation).
13. Verify no `using` directive in domain references external packages or other BCs.
14. Verify `_shared/value-object/` contains only identity/type value objects.
15. Verify `shared-kernel/` contains no domain aggregates or business logic.
16. Cross-reference engine imports against BC activation levels — no D0/D1 BC consumed by engines.
17. Verify no duplicate domain concepts across systems.
18. Verify `chain` exists only in `constitutional-system/`, `federation` only in `trust-system/identity/`.
19. Verify no domain folder uses parent-context prefix (e.g., `access-grant` forbidden).
20. Verify all namespaces follow `Whyce.Domain.<System>.<Context>.<Domain>` pattern.

## Pass Criteria

- Only canonical 11 systems + shared-kernel at root level.
- All BCs follow CLASSIFICATION > CONTEXT > DOMAIN topology (3-level minimum).
- All mandatory artifact folders present in every BC.
- All aggregates named `{Name}Aggregate`.
- All domain events follow `{Subject}{PastTenseVerb}Event` naming.
- All value objects are immutable.
- All services are stateless.
- All specifications are pure.
- No cross-BC direct references.
- No external dependencies in domain.
- No D0/D1 BC consumed by engines.
- No duplicate domain concepts across systems.
- Chain only in constitutional-system, federation only in trust-system/identity.
- Shared-kernel contains no business logic.
- All namespaces follow `Whyce.Domain.<System>.<Context>.<Domain>`.

## Fail Criteria

- Legacy or unauthorized folder at root level.
- BC does not follow three-level topology (domain directly under system).
- Missing mandatory artifact folder in a D1+ BC.
- Aggregate not named `{Name}Aggregate`.
- Domain event with present/future tense name.
- Value object with mutable property.
- Domain error that is a generic technical exception.
- Service with mutable instance field.
- Specification performing I/O or mutation.
- Cross-BC direct type reference.
- External dependency in domain code.
- Engine consuming a D0 or D1 BC.
- Duplicate domain in multiple systems without documented justification.
- Chain or federation domain in wrong system.
- Domain folder using parent-context prefix.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | External dependency in domain | `using Newtonsoft.Json;` in aggregate |
| **S0 — CRITICAL** | Cross-BC direct reference | Billing aggregate importing Identity types |
| **S1 — HIGH** | Mutable value object | `public decimal Amount { get; set; }` |
| **S1 — HIGH** | Topology violation | BC at two levels instead of three |
| **S1 — HIGH** | Engine consuming D0/D1 BC | T2E engine importing scaffold-only BC |
| **S1 — HIGH** | Duplicate domain concept | `permission` in both access and identity contexts |
| **S1 — HIGH** | Chain/federation misplacement | `chain` folder outside constitutional-system |
| **S2 — MEDIUM** | Event naming violation | `CreateOrderEvent` instead of `OrderCreatedEvent` |
| **S2 — MEDIUM** | Missing artifact folder | No `specification/` folder in D2 BC |
| **S2 — MEDIUM** | Stateful domain service | Service with `private List<>` field |
| **S2 — MEDIUM** | Prefix noise in folder name | `access-grant` instead of `grant` |
| **S2 — MEDIUM** | Namespace mismatch | Namespace doesn't match folder topology |
| **S3 — LOW** | Missing `.gitkeep` in D0 folder | Empty folder without placeholder |
| **S3 — LOW** | Specification with async method | `async Task<bool> IsSatisfied()` |

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
DOMAIN_GUARD_VIOLATION:
  bc: <classification/context/domain>
  activation_level: <D0|D1|D2>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct structure/naming>
  actual: <detected issue>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **D-PURITY-01**: Files under src/domain/** MUST NOT reference Microsoft.Extensions.DependencyInjection.* or any DI container abstraction. Domain assembly references limited to BCL + shared kernel ($7 layer purity).
- **D-DET-01**: All domain value-object ID factories are forbidden from calling Guid.NewGuid(), DateTime.Now, DateTime.UtcNow, DateTimeOffset.UtcNow, or Random. ID generation goes through injected IIdGenerator; timestamps through IClock. Applies to every *Id / value object under src/domain/**.
- **D-NO-SYSCLOCK**: No SystemClock or clock implementations in src/domain/** (delete on sight — dead code).
