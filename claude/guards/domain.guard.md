# Canonical Domain Guard

> **Authoritative source of truth for the Whycespace `src/domain/` layer.**
> This guard consolidates eleven prior guard files (domain, domain-structure, classification-suffix, dto-naming, behavioral, structural, and the five domain-aligned guards: economic, governance, identity, observability, workflow) into a single canonical document. All rule intent, severity, enforcement behaviour, and check procedures are preserved. The `domain-aligned/**` subtree has been inlined as subsections and must not be recreated as separate files.

## Purpose

Enforce DDD structural, naming, behavioral, and doctrinal conventions across all bounded contexts in `src/domain/`. Every BC must follow the canonical super-classification system architecture with CLASSIFICATION > CONTEXT > DOMAIN topology, use proper DDD artifact types, comply with domain activation levels (D0/D1/D2), and uphold WBSM v3 global enforcement (determinism, WHYCEPOLICY, WHYCECHAIN, event-first, CQRS).

## Scope

All files and folders under `src/domain/`. Cross-layer structural rules also cover `src/engines/`, `src/runtime/`, `src/systems/`, `src/platform/`, `src/projections/`, `src/shared/`, `infrastructure/`, and `tests/` where those rules bear on domain integrity. Applies to every BC, aggregate, entity, value object, event, error, service, specification, DTO, and cross-layer reference. Evaluated at CI, code review, and architectural audit.

---

## Domain Layer Purity

### Canonical Super-Classification Systems

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

### Core Purity Rules

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

### Domain Purity Extensions (2026-04-07)

- **D-PURITY-01**: Files under `src/domain/**` MUST NOT reference `Microsoft.Extensions.DependencyInjection.*` or any DI container abstraction. Domain assembly references limited to BCL + shared kernel ($7 layer purity).
- **D-DET-01**: All domain value-object ID factories are forbidden from calling `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.UtcNow`, or `Random`. ID generation goes through injected `IIdGenerator`; timestamps through `IClock`. Applies to every `*Id` / value object under `src/domain/**`.
- **D-NO-SYSCLOCK**: No `SystemClock` or clock implementations in `src/domain/**` (delete on sight — dead code).

---

## Domain Structure

Enforce the canonical nesting (`classification/context/[domain-group/]domain`) across all layers, and the `-system` suffix rule that distinguishes the domain layer from all other layers.

The physical topology is **three-or-four-level** beneath each layer root: a context may nest its domains directly (3-level form) OR group them under an explicit `domain-group/` segment (4-level form). The choice is per-context (see DS-R3 / DS-R3a). `DomainRoute` and all routing keys remain a strict three-tuple `(classification, context, domain)` regardless of physical depth (see DS-R8).

### DS-R1: Domain layer MUST use `{classification}-system`

All top-level classification folders under `src/domain/` MUST use the `-system` suffix.

**Canonical form:** `src/domain/{classification}-system/{context}/[{domain-group}/]{domain}/`

Examples:
- 3-level: `src/domain/operational-system/sandbox/todo/`
- 4-level: `src/domain/content-system/media/content-artifact/asset/`

Exception: `src/domain/shared-kernel/` is not a classification and is exempt.

Violation: Any classification folder under `src/domain/` without the `-system` suffix (e.g., `src/domain/operational/`).

### DS-R2: Non-domain layers MUST NOT use `-system`

All layers outside `src/domain/` MUST use the raw classification name WITHOUT the `-system` suffix. This applies to:

- `src/engines/`
- `src/runtime/`
- `src/systems/`
- `src/platform/`
- `src/projections/`
- `src/shared/`
- `infrastructure/`
- `tests/`

**Canonical form:** `{classification}/{context}/{domain}/` (no `-system`)

Example: `src/projections/operational/sandbox/todo/`

Violation: Any folder or namespace segment containing `-system` or `System` (PascalCase equivalent) in a non-domain layer. The only exception is `using` directives that reference `Whycespace.Domain.{X}System.*` — these correctly point into the domain layer.

### DS-R3: All paths MUST follow `classification/context/[domain-group/]domain`

Every domain concept must be reachable via three OR four levels of nesting below the layer root (or below an engine tier prefix like `T2E/`):

- **3-level form (default):** `{classification}/{context}/{domain}/`
- **4-level form (when grouping is required):** `{classification}/{context}/{domain-group}/{domain}/`

The choice is made **per context**, not per classification — a single classification MAY contain a mix of flat-context (3-level) and grouped-context (4-level) contexts.

The `domain-group/` segment is a **folder/namespace organizing concept only**. It does NOT participate in routing, policy keys, Kafka topic names, or `DomainRoute` tuples — those remain three-tuple `(classification, context, domain)` per DS-R8 and the canonical Kafka naming pattern.

Violation: A domain placed directly under a classification without a context level (e.g., `src/projections/operational/todo/` — missing the `sandbox` context).

If the context cannot be inferred, use `default` as a temporary context.

### DS-R3a: Domain-group adoption discipline (canonical sub-clause of DS-R3, locked 2026-04-20)

When a context introduces a `domain-group/` layer, the following discipline applies:

1. **Per-context atomicity.** A context is either fully flat (every domain sits directly under the context) OR fully grouped (every domain sits under exactly one `domain-group/`). Mixing both shapes inside a single context is **FORBIDDEN** — `context/groupA/domainX/` cannot coexist with `context/domainY/`.
2. **Adoption trigger.** A context SHOULD adopt the 4-level form when its domains fall into two or more semantic groups whose names carry meaning beyond mere alphabetical sorting (e.g., `content-artifact` vs `companion-artifact` vs `lifecycle` vs `descriptor`). Adoption is RECOMMENDED, not mandatory; arbitrary or unmotivated grouping is a structural drift signal.
3. **Single-domain-group representation.** When a context conceptually has only one group with one or two domains, it SHOULD remain flat (3-level). It MAY use a single named domain-group ONLY when (a) the group represents a defined semantic category that is documented to admit future siblings (e.g., `streaming/control/access` foresees additional control domains) AND (b) the group name is meaningful in its own right.
4. **No gratuitous nesting.** A `domain-group/` whose name is generic (`misc/`, `other/`, `default/`, `core/` used solely as a fallback) is a violation. Group names must carry domain semantics. The `default` context fallback in DS-R3 does NOT extend to domain-group naming.
5. **Mandatory artifact placement.** The 7 mandatory artifact subfolders (`aggregate/`, `entity/`, `error/`, `event/`, `service/`, `specification/`, `value-object/`) sit on the **leaf domain folder**, never on the domain-group folder. The domain-group folder contains only domain folders (and optionally a README).
6. **Active classifications using the 4-level form (registry):** `content-system` (every context: `document`, `media`, `streaming`). All other classifications remain 3-level. New adopters MUST update this registry in the same PR that introduces the topology.

### DS-R4: No flat domains allowed

A classification folder that contains domain folders directly (without a context intermediary) is a structural violation.

Violation: `src/projections/operational/todo/` instead of `src/projections/operational/sandbox/todo/`.

### DS-R5: Cross-layer mapping MUST be consistent

For every domain that exists in `src/domain/{classification}-system/{context}/[{domain-group}/]{domain}/`, the corresponding paths in other layers MUST mirror the **same depth** (3-level or 4-level) with the raw classification (no `-system`):

- `src/engines/T2E/{classification}/{context}/[{domain-group}/]{domain}/`
- `src/projections/{classification}/{context}/[{domain-group}/]{domain}/`
- `src/systems/downstream/{classification}/{context}/[{domain-group}/]{domain}/`
- `infrastructure/policy/domain/{classification}/{context}/[{domain-group}/]{domain}/`
- `infrastructure/event-fabric/kafka/topics/{classification}/{context}/[{domain-group}/]{domain}/` (folder layout only — topic names remain 3-tuple per K-TOPIC-CANONICAL-01)
- `infrastructure/data/postgres/projections/{classification}/`

A context that uses the 4-level form in `src/domain/` MUST use the 4-level form in EVERY other layer. A context that is 3-level in `src/domain/` MUST be 3-level in every other layer. Cross-layer depth drift is a violation.

Violation: Mismatched classification, context, domain-group (when applicable), or domain segments across layers.

### DS-R6: Namespace consistency with folder structure

C# namespaces MUST reflect the canonical folder path. Non-domain namespaces must use `Orchestration` (not `OrchestrationSystem`), `Constitutional` (not `ConstitutionalSystem`), etc.

Domain namespaces correctly use the PascalCase equivalent of the `-system` suffix: `Whycespace.Domain.OrchestrationSystem.*`.

When a context uses the 4-level form, the namespace MUST include the PascalCase `domain-group` segment between context and domain: `Whycespace.Domain.{Classification}System.{Context}.{DomainGroup}.{Domain}`. Hyphens in the domain-group folder name (e.g. `content-artifact`) become PascalCase joins (`ContentArtifact`).

### DS-R7: Residual `-system` suffix sweep tracking

The following non-domain paths still carry the `-system` suffix (DS-R2 violations deferred for bounded-blast-radius remediation). These MUST be renamed in follow-up sweeps:

**Shared contracts:** `src/shared/contracts/events/orchestration-system/`, `src/shared/contracts/projections/orchestration-system/`
**Tests:** `tests/integration/constitutional-system/`, `tests/integration/orchestration-system/`, `tests/unit/orchestration-system/`

Suggested sweep order (one PR per classification):
1. `constitutional-system` → `constitutional` (folder + namespace)
2. `economic-system` → `economic`
3. `decision-system` → `decision`
4. `orchestration-system` → `orchestration` (touches projections + shared contracts + tests — largest)
5. `business-system`, `core-system`, `intelligence-system`, `structural-system`, `trust-system`, `governance-system`, `identity-system` — single sweep

Note: `src/domain/*-system` directories are CORRECT per DS-R1 and are NOT part of this remediation.

### DS-R8: Classification strings in DomainRoute and CommandContext

`DomainRoute` and `CommandContext.Classification` MUST use the raw classification name without `-system`.

**Canonical:** `DomainRoute("operational", "sandbox", "todo")`

Violation: `DomainRoute("operational-system", "sandbox", "todo")` or `Classification = "orchestration-system"`.

`DomainRoute` is and remains a **strict three-tuple** `(classification, context, domain)` even when the underlying folder layout uses the 4-level form (DS-R3a). The `domain-group` segment is intentionally absent from the routing key so that policy IDs, Kafka topic names, projection schemas, and consumer-group names remain stable when grouping evolves. A 4-level domain `content-system/media/content-artifact/asset/` routes as `DomainRoute("content", "media", "asset")` — never as `DomainRoute("content", "media.content-artifact", "asset")` and never as a four-tuple.

### Domain Structure Severity

- DS-R1 through DS-R5: **S1** (architectural — structural drift)
- DS-R3a: **S1** (architectural — half-adopted or gratuitous grouping breaks per-context atomicity)
- DS-R6, DS-R8: **S2** (structural — naming/reference inconsistency)
- DS-R7: **S2** (structural drift — tracked remediation backlog)

---

## Classification Suffix Conventions

### CLASS-SFX-R1 — Domain Suffix Required

All classification folders directly under `src/domain/` MUST end with `-system`.

**Scan:** `src/domain/` (top-level directories only, excluding `bin`, `obj`, `shared-kernel`)
**Assert:** Every classification folder name ends with `-system`.
**Severity:** S0 (build blocking)

### CLASS-SFX-R2 — Non-Domain Suffix Prohibited

Only `src/domain/**` may contain `-system` in folder names.
All other `src/**` directories MUST NOT contain `-system`.

**Scan:** `src/{engines,runtime,systems,platform,projections,shared}/**`
**Assert:** No directory path segment ends with `-system`.
**Severity:** S0 (build blocking)

### CLASS-SFX-R3 — Enforcement

Violations of CLASS-SFX-R1 or CLASS-SFX-R2 are S0 severity.
Any prompt that would introduce a `-system` suffix outside `src/domain/` or remove one inside `src/domain/` MUST be halted before execution.

---

## DTO Naming

### Objective

Ensure all Data Transfer Objects (DTOs) follow a consistent, clear, and canonical naming convention across the system. This prevents ambiguity between API transport models, domain models, and projection/read models.

### Core Principle

DTO names must clearly indicate their role:
- Input (Request)
- Output (Response)
- Stored state (ReadModel)

No DTO should have ambiguous intent.

### Naming Rules

#### 1. API Request DTOs

Pattern: `{Action}{Domain}RequestModel`

Examples:
- `CreateTodoRequestModel`
- `UpdateTodoRequestModel`
- `CreateCardRequestModel`

#### 2. API Response DTOs

Pattern: `{Action}{Domain}ResponseModel` or `Get{Domain}ResponseModel`

Examples:
- `CreateTodoResponseModel`
- `GetTodoResponseModel`
- `CreateBoardResponseModel`

#### 3. Query Naming Alignment

Queries must match response naming:
- `GetTodoQuery` → `GetTodoResponseModel`
- `GetKanbanBoardQuery` → `GetKanbanBoardResponseModel`

#### 4. Projection Models (Read Models)

Pattern: `{Domain}ReadModel`

Examples:
- `TodoReadModel`
- `KanbanBoardReadModel`
- `WorkflowExecutionReadModel`

### Critical Distinction

ReadModel ≠ ResponseModel

| Type          | Purpose                                |
| ------------- | -------------------------------------- |
| ReadModel     | Internal storage (projection/database) |
| ResponseModel | External API contract                  |

### Redundancy Rule

If a DTO is already scoped by folder (e.g. `/operational/sandbox/kanban/`) DO NOT repeat the domain in the name:

- Correct: `CreateCardRequestModel`
- Incorrect: `CreateKanbanCardRequestModel`

### Prohibited Patterns

The following are NOT allowed:
- `*Dto`
- `*Response` (without `Model` suffix)
- `*Request` (without `Model` suffix)
- `*Data`
- ambiguous names like: `TodoModel`, `CardInfo`, `BoardData`

### DTO Enforcement Rules

- **DTO-R1 — Role Clarity**: Every DTO must clearly be `RequestModel`, `ResponseModel`, or `ReadModel`.
- **DTO-R2 — Naming Consistency**: All DTOs must follow defined patterns exactly.
- **DTO-R3 — No Domain Duplication**: Domain name must not be repeated if already implied by folder structure.
- **DTO-R4 — No Ambiguous DTOs**: DTOs must not be confused with domain models or projections.

### DTO Violation Severity

| Severity | Description                                |
| -------- | ------------------------------------------ |
| S0       | Ambiguous DTO usage (breaks understanding) |
| S1       | Naming inconsistency                       |
| S2       | Cosmetic deviation                         |

### DTO Action

- S0 → MUST be fixed immediately
- S1 → SHOULD be fixed
- S2 → optional cleanup

### Canonical DTO Principle

> If a developer cannot tell whether a type is a Request, Response, or ReadModel from its name, it is invalid.

### DTO Scope

Applies to:
- `src/platform/api`
- `src/shared/contracts`
- `src/projections`
- all DTO definitions

DTO naming is not cosmetic. It defines system readability, developer onboarding speed, and correctness of API boundaries. This guard ensures the system remains clear and consistent as it scales.

---

## Behavioral Rules

Enforce runtime behavior rules that govern how components interact at execution time. Structural correctness alone is insufficient — this guard ensures that the system behaves according to WBSM v3 doctrine during operation: correct flow direction, proper isolation, and deterministic execution paths.

**Scope:** All executable code under `src/`. Applies to method bodies, constructor logic, event handlers, and service implementations. Evaluated at code review, CI, and integration test phases.

### Behavioral Rule Set

1. **NO DIRECT DB FROM DOMAIN** — Domain layer must never perform direct database access. No `DbContext`, `IDbConnection`, `SqlCommand`, repository implementation, or ORM calls in `src/domain/`. Domain defines repository interfaces only; implementations live in `infrastructure/`.

2. **NO ENGINE-TO-ENGINE CALLS** — Engines must not invoke other engines directly. An engine at any tier (T0U-T4A) must not hold a reference to or call methods on another engine. Cross-engine coordination flows through runtime only.

3. **NO DOMAIN MUTATION FROM SYSTEMS** — The systems layer must never modify domain state. Systems compose and orchestrate but do not create, update, or delete domain aggregates. All state changes originate from engines invoked through runtime.

4. **NO BYPASS OF RUNTIME CONTROL PLANE** — All command and event routing must pass through the runtime layer. No component may short-circuit the control plane by directly invoking domain services or engine operations outside the runtime pipeline.

5. **COMMANDS FLOW THROUGH RUNTIME ONLY** — Command objects must be dispatched exclusively through runtime command handlers. No direct instantiation and execution of command handlers outside the runtime pipeline. Platform dispatches commands to runtime; runtime routes to engines.

6. **EVENTS ARE IMMUTABLE AFTER PUBLISH** — Once a domain event is published (raised by an aggregate and dispatched by runtime), its properties must not be modified. Event classes must be immutable (readonly properties, no setters). No post-publish mutation is permitted.

7. **NO SYNCHRONOUS CROSS-BC CALLS** — Bounded contexts must not call each other synchronously. Cross-BC communication must use domain events (asynchronous). No direct method invocation between aggregates in different BCs. Integration events bridge BC boundaries.

8. **DETERMINISTIC COMMAND HANDLING** — Command handlers must be deterministic: same input produces same side effects. No reliance on ambient state, current time (must be injected), or random values (must be seeded/injected). System clock access must use an injectable `ITimeProvider`.

9. **NO SIDE EFFECTS IN QUERIES** — Query handlers and read operations must not produce side effects. No state mutation, no event publishing, no command dispatching from within a query handler or projection read.

10. **AGGREGATE TRANSACTION BOUNDARY** — Each command targets exactly one aggregate. A single transaction must not span multiple aggregates. Cross-aggregate consistency is achieved through eventual consistency via domain events.

11. **NO STATIC MUTABLE STATE** — No static mutable fields or properties in domain, engine, or runtime code. Static state breaks isolation, testability, and multi-tenancy. Only immutable constants and pure static methods are permitted.

12. **EXCEPTION BOUNDARY ENFORCEMENT** — Domain exceptions must not leak to platform. Engines catch domain exceptions and translate to result types. Runtime translates engine results to platform-consumable responses. No raw exception propagation across layers.

13. **POLICY DECISION REQUIRED BEFORE EXECUTION** — Every command must include a `PolicyDecision` in its execution context before reaching the engine. Commands without an attached, evaluated policy decision must be rejected at the runtime pipeline. No engine may execute a command that lacks a policy decision artifact.

14. **CHAIN LINK REQUIRED** — Every execution that mutates domain state must produce a `ChainBlock` entry linking the action to the immutable ledger. The chain link includes: action hash, policy decision hash, actor, timestamp, and correlation ID. Executions without chain anchoring are governance violations.

15. **EVENT MUST FOLLOW EVERY STATE CHANGE** — No domain mutation may occur without emitting at least one domain event. Silent state changes (direct field assignment without event) are forbidden. Every `Apply()`, `Create()`, `Update()`, or state transition must raise a corresponding past-tense domain event.

16. **NO CLOCK OR RANDOM ACCESS** — All temporal and identity generation must use injected providers (`ITimeProvider`, `IIdGenerator`). Direct use of `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `Guid.NewGuid()`, `Random`, or `Environment.TickCount` in domain, engine, or runtime code is forbidden.

### Behavioral Check Procedure

1. Scan `src/domain/` for any database-related types (`DbContext`, `IDbConnection`, `SqlCommand`, `EntityFrameworkCore`, `Dapper`, `NpgsqlConnection`).
2. Scan `src/engines/` for imports or instantiations of other engine types.
3. Scan `src/systems/` for aggregate method calls that mutate state (`.Create()`, `.Update()`, `.Apply()`, `.RaiseEvent()`).
4. Scan for command handler invocations outside `src/runtime/` pipeline.
5. Verify all domain event classes have only `{ get; }` or `{ get; init; }` properties — no `{ get; set; }`.
6. Scan for direct method calls between aggregates in different BC namespaces.
7. Verify command handlers do not call `DateTime.Now`, `DateTime.UtcNow`, `Guid.NewGuid()` directly — must use injected providers.
8. Scan query handlers for write operations, event publishing, or command dispatching.
9. Verify each command handler targets a single aggregate repository.
10. Scan for `static` fields with mutable types (`static List<>`, `static Dictionary<>`, `static int`, etc.) in domain/engine/runtime.
11. Verify domain exception types are caught at engine or runtime boundary.

### Behavioral Pass Criteria

- Zero forbidden patterns detected across all scans.
- All domain events are immutable.
- All command handlers are deterministic (no ambient state access).
- All cross-BC communication uses async events.
- No engine-to-engine direct invocation found.
- No domain mutation found in systems layer.
- All commands routed through runtime.

### Behavioral Fail Criteria

- Domain file contains direct database access code.
- Engine file instantiates or calls another engine.
- Systems file mutates domain aggregate state.
- Command handler invoked outside runtime pipeline.
- Domain event class has mutable property (`{ get; set; }`).
- Synchronous method call crosses BC boundary.
- Command handler uses `DateTime.Now` or `Guid.NewGuid()` directly.
- Query handler produces side effects.
- Single transaction spans multiple aggregates.
- Static mutable state detected in domain/engine/runtime.

### Behavioral Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Direct DB access in domain | `_dbContext.SaveChanges()` in aggregate |
| **S0 — CRITICAL** | Domain mutation from systems | `aggregate.Apply(event)` in systems layer |
| **S0 — CRITICAL** | Mutable domain event | `public string Name { get; set; }` on event |
| **S0 — CRITICAL** | Engine persistence attempt | `context.Save()`, `_repository.Save()`, `DbContext.SaveChanges()` in engine |
| **S0 — CRITICAL** | Engine publishing event externally | Engine calls `eventBus.Publish()`, `IEventPublisher`, Kafka produce |
| **S0 — CRITICAL** | Engine calling infra | Engine calls Redis, Kafka, HTTP, SQL, file I/O directly |
| **S0 — CRITICAL** | Runtime bypass | Any path invoking engine without RuntimeControlPlane middleware pipeline |
| **S1 — HIGH** | Engine-to-engine call | T2E engine instantiating T1M engine |
| **S1 — HIGH** | Synchronous cross-BC call | `identityService.GetUser()` from billing aggregate |
| **S2 — MEDIUM** | Non-deterministic handler | `DateTime.UtcNow` in command handler |
| **S2 — MEDIUM** | Multi-aggregate transaction | Two repository saves in one handler |
| **S3 — LOW** | Static mutable state | `static int counter` in engine |
| **S3 — LOW** | Exception leakage | Domain exception reaching platform unhandled |

### Behavioral Violation Report Format

```
BEHAVIORAL_GUARD_VIOLATION:
  file: <path>
  method: <method name>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  pattern_detected: <code snippet>
  remediation: <fix instruction>
```

### Behavioral Extensions (2026-04-07)

- **B-CLOCK-01**: `SystemClock` is the SINGLE permitted reader of `DateTimeOffset.UtcNow`. All other code MUST consume `IClock`. Sandbox-mode `IClock` implementations MUST accept an injectable/seeded time source for deterministic replay.
- **B-ID-01**: All correlation/causation/aggregate ID generation MUST go through `IIdGenerator`. `Guid.NewGuid()` is FORBIDDEN in `src/runtime/**`, `src/engines/**`, `src/domain/**`, AND `src/platform/host/adapters/**`.
- **B-CHAIN-01**: `ChainAnchor.BlockId` MUST be a deterministic hash of (events, previousBlockId). Timestamps from `IClock` only. No `Guid.NewGuid()` for block ids.

### Behavioral Extensions (2026-04-15 / 2026-04-16)

- **D-VO-TYPING-01 — Strongly-Typed Identifier Discipline (S2 for Guid, S1 for string)**: Within any bounded context in `src/domain/**`, if a strongly-typed identifier value object (e.g. `AccountId`, `OwnerId`, `TargetId`, `AllocationId`) exists in that bounded context's `value-object/` folder OR in an adjacent bounded context within the same classification, then:
  1. Aggregate properties that hold that identifier MUST use the VO type, not `Guid` or `string`.
  2. Domain event record members that hold that identifier MUST use the VO type, not `Guid` or `string`.
  3. Factory methods and aggregate behavior methods MUST accept the VO type, not `Guid` or `string`.
  4. Service methods and specification constructors that accept that identifier MUST use the VO type.

  **Exception:** Legitimate external/untyped correlation identifiers (e.g. HTTP correlation ids from outside the domain) MAY be represented as `Guid` or `string` at the domain boundary provided there is a documented comment justifying it. Entry-point factories that receive raw `Guid` from the runtime layer MUST wrap into the VO on the first line of the method.

  **Scan:** grep for `public Guid \w+Id`, `Guid \w+Id[,)]`, `string \w+Id[,)]` across `src/domain/**`, cross-reference against sibling `value-object/*.cs` files. **Rationale:** Primitive-obsession for identifiers breaks the DDD contract that value objects encode invariants (non-empty, format, etc.); also makes event payloads ambiguous during replay and projection — two different ids of the same primitive type can be silently swapped. Strengthens rules 5, 14, 23.

- **D-ERR-TYPING-01 — Framework Exception Types Forbidden in Domain (S1)**: Files under `src/domain/**` MUST NOT directly throw or construct any of the following BCL exception types: `System.ArgumentException`, `System.ArgumentNullException`, `System.ArgumentOutOfRangeException`, `System.InvalidOperationException`, `System.NotSupportedException`, `System.NotImplementedException`, `System.Exception`. Instead, domain code MUST:
  1. Use the shared-kernel `Guard.Against(...)` helper, which raises a `DomainException`.
  2. Use a context-specific `{Bc}Errors` static factory (e.g. `CapitalAccountErrors.InvalidAmount()`) that returns a `DomainException` / `DomainInvariantViolationException`.

  **Scan:** grep `src/domain/**` for `throw new (ArgumentException|ArgumentNullException|ArgumentOutOfRangeException|InvalidOperationException|NotSupportedException|NotImplementedException|Exception)\b`. **Rationale:** Domain exceptions carry business meaning. Framework exceptions bleed technical/infrastructural semantics across the layer boundary and break behavioral rule 12 (Exception Boundary Enforcement). Clarifies the intent of Core Purity rule 7.

- **DOM-LIFECYCLE-INIT-IDEMPOTENT-01 — Lifecycle-Init Idempotency (S2)**: Every aggregate's lifecycle-init action (`Open*`, `Create*`, `Initialize*`, factory-style first event) MUST refuse to emit a second initialization event on an already-loaded aggregate. Canonical guard:

  ```csharp
  public void OpenOrCreate(...)
  {
      if (Version >= 0) throw <Aggregate>Errors.AlreadyInitialized();
      RaiseDomainEvent(new <Aggregate>InitializedEvent(...));
  }
  ```

  A specification class encoding the check (e.g., `AlreadyOpenSpecification`, `AlreadyCreatedSpecification`) is the canonical form and goes alongside other specs under `<aggregate>/specification/`. `AggregateRoot.Version` starts at `-1` and increments on `LoadFromHistory`; `Version >= 0` is an authoritative "already loaded" discriminator that does not require domain-specific state shape.

  **Scan:** enumerate aggregates under `src/domain/**/aggregate/` and verify each method that raises a "first event" (event-type matches `*Opened|*Created|*Initialized`) checks `Version >= 0` (or equivalent). **Rationale:** Without this guard, re-issuing the same lifecycle-init command produces duplicate seed events in the write-side stream even when projection-layer idempotency masks the symptom. Complements INV-001 (Command Outcome Totality) and INV-303 (Replay-Safe). Paired with the constitutional `INV-IDEMPOTENT-LIFECYCLE-INIT-01` (engine-handler shape for static-factory aggregates).

---

## Structural Rules

Enforce repository partition boundaries and layer purity across the WBSM v3 architecture. No layer may reach into another layer's internal structure, and dependency direction must flow inward (platform > systems > runtime > engines > domain).

**Scope:** All files under `src/`. Applies to every commit, PR, and CI pipeline run.

### Structural Rule Set

1. **DOMAIN ISOLATION** — `src/domain/` has ZERO external dependencies. No NuGet packages, no infrastructure imports, no framework references. Domain references only `src/shared/` kernel primitives. No `using` directive may reference any namespace outside `Domain` or `Shared.Kernel`.

2. **SHARED PURITY** — `src/shared/` contains ONLY contracts, kernel primitives, and cross-cutting value types. It must NOT contain business logic, domain rules, workflow orchestration, or persistence concerns. Shared may not reference domain, engines, runtime, systems, or platform.

3. **ENGINE TIER COMPLIANCE** — `src/engines/` follows the T0U-T4A tiered topology. Engines import from `src/domain/` and `src/shared/` only. Engines NEVER define domain aggregates, entities, value objects, or domain events. Engines never reference other engines, runtime, systems, or platform.

4. **RUNTIME BOUNDARY** — `src/runtime/` is the control plane, middleware, and internal projection layer. Runtime may reference engines, domain, and shared. Runtime must NOT contain domain model definitions. Runtime must NOT contain direct persistence logic (only internal projection handlers). Runtime must NOT reference `src/projections/`.

5. **SYSTEMS COMPOSITION** — `src/systems/` is composition-only. Systems compose engines via runtime references. Systems must NOT contain execution logic, domain model definitions, or direct persistence. Systems may reference runtime and shared only.

6. **PLATFORM ENTRY** — `src/platform/` is the entry layer (API controllers, CLI, host configuration). Platform must NOT reference engines directly. Platform must NOT contain direct database access. Platform references systems and runtime only.

7. **DOMAIN PROJECTIONS LAYER** — `src/projections/` is the domain projection layer (CQRS read models / query layer). Projections may reference ONLY `src/shared/` and `infrastructure/` adapters. Projections must NOT reference `src/domain/`, `src/runtime/`, `src/engines/`, `src/systems/`, or `src/platform/`. All domain projections are event-driven only (Kafka/event fabric). Redis/read-store writes originate ONLY from this layer.

8. **INFRASTRUCTURE ADAPTERS** — `infrastructure/` contains only adapter implementations (repositories, messaging, external service clients). Infrastructure must NOT contain business logic or domain rules. Infrastructure implements interfaces defined in domain or shared.

9. **DEPENDENCY DIRECTION** — Dependencies flow strictly: `platform > systems > runtime > engines > domain < shared`. `src/projections/` is an isolated read-side layer referencing only `shared` and `infrastructure`. No reverse dependency is permitted. No lateral dependency within the same layer (e.g., engine-to-engine). Runtime and projections are mutually isolated — neither may reference the other.

10. **NAMESPACE ALIGNMENT** — File namespace must match its physical folder path. A file in `src/domain/economic-system/capital/vault/` must have namespace `Domain.EconomicSystem.Capital.Vault.*`. No namespace aliasing to circumvent layer boundaries.

11. **NO TRANSITIVE LEAKAGE** — Public types in inner layers must not expose types from outer layers in their signatures. Domain types must not reference engine or runtime types even transitively.

12. **TEST LAYER MUST MIRROR DOMAIN** — The test project structure must mirror the domain structure. For each BC at `src/domain/{system}/{context}/{domain}/`, a corresponding test folder must exist at `tests/{system}/{context}/{domain}/`. Test classes must follow the naming pattern `{ClassName}Tests`. Missing test mirrors for D2-level BCs are a structural violation.

13. **NO BUSINESS LOGIC OUTSIDE DOMAIN** — Business rules, domain invariants, and business decision logic must reside exclusively in `src/domain/`. No business logic in engines (engines execute domain operations, not define rules), runtime (runtime orchestrates, not decides), systems (systems compose, not compute), platform (platform routes, not validates), shared (shared carries types, not logic), infrastructure (infrastructure adapts, not reasons), or projections (projections flatten and denormalize, not decide).

14. **EVENT STORE IS SOURCE OF TRUTH** — The event store is the canonical source of truth for all domain state. Aggregate state is derived by replaying events. No alternative persistence mechanism may serve as the authoritative state source. Read models, caches, and projections are derived views — never authoritative. If the event store and a projection disagree, the event store is correct.

15. **DUAL PROJECTION MODEL** — WBSM v3 enforces two isolated projection layers:
    - `src/runtime/projection/` — internal execution support (workflow state, idempotency tracking, policy linking). NOT exposed externally. May be synchronous.
    - `src/projections/` — domain read models (CQRS query layer). Event-driven ONLY. The sole query source for APIs. Owns Redis/read-store writes.
    Both layers are mandatory, isolated, and non-overlapping. Any cross-reference between them is a CRITICAL violation.

16. **DOMAIN CLASSIFICATION REGISTRY (LOCKED)** — The top-level folders under `src/domain/` are the **canonical classification set**. Each classification uses the `{name}-system` suffix. The locked registry is:

    | Classification | Folder | Namespace Prefix |
    |---|---|---|
    | Business | `business-system` | `BusinessSystem` |
    | Constitutional | `constitutional-system` | `ConstitutionalSystem` |
    | Core | `core-system` | `CoreSystem` |
    | Decision | `decision-system` | `DecisionSystem` |
    | Economic | `economic-system` | `EconomicSystem` |
    | Intelligence | `intelligence-system` | `IntelligenceSystem` |
    | Operational | `operational-system` | `OperationalSystem` |
    | Orchestration | `orchestration-system` | `OrchestrationSystem` |
    | Structural | `structural-system` | `StructuralSystem` |
    | Trust | `trust-system` | `TrustSystem` |

    **Special entries** (not classifications): `shared-kernel` (domain primitives), `bin/`, `obj/` (build artifacts).

    **Creating a new classification folder under `src/domain/` requires explicit user approval.** Claude Code MUST NOT create, rename, or remove classification-level folders without direct instruction. Violating this is an **S0 — CRITICAL** structural breach.

    All domain paths follow `src/domain/{classification-system}/{context}/{domain}/`. No domain code may exist directly under a classification folder — it must be nested in a context.

### Structural Check Procedure

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
16. Verify all directories directly under `src/domain/` are in the locked classification registry (Rule 16). Flag any unrecognized folder as S0.

### Structural Pass Criteria

- All dependency arrows flow in the permitted direction.
- Zero cross-layer import violations detected.
- Zero domain model definitions found outside `src/domain/`.
- Zero business logic found in `src/shared/` or `infrastructure/`.
- All namespaces align with physical folder structure.
- `src/projections/` exists with domain-aligned projection modules.
- `src/runtime/projection/` exists with execution-support projections.
- Zero cross-references between runtime projections and domain projections.
- Domain projections reference only `src/shared/`.

### Structural Fail Criteria

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

### Structural Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Domain imports external dependency | `using Microsoft.EntityFrameworkCore;` in domain |
| **S0 — CRITICAL** | Reverse dependency direction | Engine importing from runtime |
| **S1 — HIGH** | Domain model defined outside domain | Aggregate class in `src/engines/` |
| **S1 — HIGH** | Business logic in shared or infrastructure | If/else domain rules in `src/shared/` |
| **S0 — CRITICAL** | Domain projections reference runtime/domain/engines | `using Whycespace.Runtime;` in `src/projections/` |
| **S0 — CRITICAL** | Runtime projections reference domain projections | `using Whycespace.Projections;` in `src/runtime/projection/` |
| **S0 — CRITICAL** | Missing projection layer | `src/projections/` or `src/runtime/projection/` does not exist |
| **S0 — CRITICAL** | Unauthorized classification folder created/renamed/removed | New folder `src/domain/analytics-system/` without approval |
| **S2 — MEDIUM** | Platform referencing engine directly | `using Engines.T2E.*;` in platform controller |
| **S2 — MEDIUM** | Namespace misalignment | File path and namespace disagree |
| **S3 — LOW** | Unnecessary transitive reference | Shared referencing a type it does not use |

### Structural Violation Report Format

```
STRUCTURAL_GUARD_VIOLATION:
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct behavior>
  actual: <detected behavior>
```

### Structural Extensions (2026-04-07)

- **STR-AUTH-01**: Every controller action with HTTP verb POST/PUT/PATCH/DELETE MUST be protected by `[Authorize]` (controller or action) OR by global authentication middleware registered before MVC routing. A structural scan MUST fail the build for any unprotected mutating endpoint.
- **STR-HEALTH-01**: Health-check adapter implementations live in `src/infrastructure/health/`. Contracts (`IHealthCheck`, `HealthCheckResult`) live in `src/shared/contracts/infrastructure/health/`. `HealthAggregator` lives in infrastructure and is exposed only via platform API.
- **STR-OBS-01**: Observability middleware (metrics/logging/tracing) lives ONLY in `src/runtime/observability/`. MUST NOT contain business logic or domain references.

### Structural Extensions (2026-04-08 — guard-registry drift)

- **STR-GUARD-REGISTRY-01** (S2): CLAUDE.md $1a guard list drifts from on-disk `claude/guards/*.guard.md`. The canonical loading semantics MUST be "load every `*.guard.md` under `/claude/guards/`" rather than an explicit enumeration, so the set self-heals against future guard additions. Any explicit enumeration in CLAUDE.md $1a MUST match the on-disk set exactly, or be replaced with the glob directive. Phase-1 authored guards (clean-code, composition-loader, dependency-graph, determinism, deterministic-id, hash-determinism, platform, program-composition, replay-determinism, runtime-order) are canonical. Source: `claude/new-rules/_archives/20260408-090519-guards.md`.

---

## Domain-Aligned Guards

Doctrinal guards tied to specific domain subtrees. These were previously stored as separate files under `claude/guards/domain-aligned/` and are inlined here as subsections. They must NOT be re-split into separate files.

### Economic

Doctrinal guard for `src/domain/economic-system/`. Covers event sourcing, CQRS boundary, invariant enforcement, and per-context rules (transaction, revenue, exposure, reconciliation, enforcement, compliance, exchange).

#### Core Economic Rules (2026-04-07 Normalization)

##### RULE: ECON-ES-01 — EVENT SOURCING IS SOURCE OF TRUTH
All economic state changes MUST occur via domain events. No direct state mutation is allowed outside aggregate methods.

ENFORCEMENT:
- Aggregates must raise events before state change
- No repository-level mutation logic allowed

##### RULE: ECON-CQRS-01 — READ/WRITE SEPARATION
Write model (aggregates) and read model (projections) MUST be separated.

ENFORCEMENT:
- `src/domain` MUST NOT depend on projections
- `src/projections` MUST NOT mutate domain state

##### RULE: ECON-LEDGER-01 — INVARIANT ENFORCEMENT
Economic aggregates MUST enforce invariant checks BEFORE emitting events.

ENFORCEMENT:
- Ledger must balance
- Allocation/reserve must validate constraints before event emission

#### Transaction Context Rules (2026-04-10)

##### T-RULES (Transaction Flow)

**T1 — TRANSACTION OUTPUT RULE**
Every transaction MUST produce exactly one journal. A completed `TransactionAggregate` with an empty `JournalId` is an invariant violation. Enforced in `TransactionAggregate.EnsureInvariants()`.

**T2 — NO DIRECT ENTRY CREATION**
Transaction cannot create entries directly. The flow is: Transaction → Journal → Entries. Any code path in the transaction context that creates `LedgerEntryAggregate` instances or raises `LedgerEntryRecordedEvent` is a **CRITICAL violation**.

**T3 — LIMIT VALIDATION FIRST**
Transaction must validate limits before execution. `LimitAggregate.Check()` must be called BEFORE `TransactionAggregate.Complete()`. Calling `Complete()` without prior limit validation is a flow violation.

**T4 — CHARGE BEFORE EXECUTION**
Charges must be calculated and applied before journal creation. `ChargeAggregate.Calculate()` and `ChargeAggregate.ApplyCharge()` must complete BEFORE `TransactionAggregate.Complete()`. Charges feed into the journal entries.

**T5 — INSTRUCTION REQUIRED**
Transaction must originate from an instruction. `TransactionAggregate.Initiate()` requires a non-empty `InstructionId`. A transaction without an instruction reference is a **CRITICAL violation**.

##### Transaction D-RULES (Domain Constraints)

**D41 — TRANSACTION REQUIRES JOURNAL**
Every transaction must produce a journal. Validated by `TransactionJournalLinkService.ValidateJournalProduced()` and enforced as an aggregate invariant.

**D42 — NO ENTRY CREATION IN TRANSACTION**
Entries must only be created via journal. The transaction context MUST NOT contain any reference to `LedgerEntryAggregate`, `LedgerEntryRecordedEvent`, or entry-level domain types.

**D43 — INSTRUCTION-FIRST FLOW**
Transactions must originate from instruction. `TransactionAggregate.Initiate()` enforces non-empty `InstructionId`.

**D44 — LIMIT ENFORCEMENT**
Transactions must validate limits before execution. `LimitCheckedEvent` → `TransactionCompletedEvent`. If `LimitExceededEvent`, the transaction MUST NOT complete.

**D45 — CHARGE APPLICATION**
Charges must be applied before execution. `ChargeCalculatedEvent` → `ChargeAppliedEvent` → `TransactionCompletedEvent`.

##### Transaction C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C26** | Transaction completed without journal | S0 — CRITICAL |
| **C27** | Entry created outside journal flow | S0 — CRITICAL |
| **C28** | Transaction created without instruction | S0 — CRITICAL |
| **C29** | Transaction completed without limit validation | S1 — HIGH |
| **C30** | Transaction completed without charge application | S1 — HIGH |

##### Canonical Transaction Execution Order

```
1. Instruction created          (TransactionInstructionCreatedEvent)
2. Limits checked               (LimitCheckedEvent)
3. Charge calculated            (ChargeCalculatedEvent)
4. Charge applied               (ChargeAppliedEvent)
5. Transaction initiated        (TransactionInitiatedEvent)
6. Transaction completed        (TransactionCompletedEvent — includes JournalId)
7. Instruction marked executed  (TransactionInstructionExecutedEvent)
```

Steps 2-4 MUST complete before step 6. Step 1 MUST precede step 5. Step 6 MUST include a journal reference.

##### Transaction Check Procedure

1. Scan `TransactionAggregate.Complete()` — verify it requires non-empty `JournalId` parameter.
2. Scan `TransactionAggregate.Initiate()` — verify it requires non-empty `InstructionId`.
3. Scan `TransactionAggregate.EnsureInvariants()` — verify Completed status requires JournalId.
4. Grep transaction context for `LedgerEntryAggregate` or `LedgerEntryRecordedEvent` — must find zero results.
5. Verify `LimitAggregate.Check()` raises `LimitExceededEvent` and throws on breach.
6. Verify `ChargeAggregate.ApplyCharge()` transitions from Calculated to Applied.

##### Transaction Fail Criteria

- `TransactionAggregate` allows completion without `JournalId` → **C26**
- Any file in `src/domain/economic-system/transaction/` references entry-level types → **C27**
- `TransactionAggregate` allows initiation without `InstructionId` → **C28**
- No limit check mechanism exists or is bypassable → **C29**
- Charge can be skipped in execution flow → **C30**

#### Revenue Context Rules (2026-04-10)

##### R-RULES (Revenue Flow)

**R1 — CONTRACT REQUIRED**
Revenue cannot exist without a contract. `RevenueAggregate.Recognize()` requires a non-empty `ContractId`. Revenue without a contract origin is a **CRITICAL violation**.

**R2 — SHARE CONSISTENCY**
Distribution must equal total revenue. `DistributionAggregate.EnsureInvariants()` enforces that the sum of all allocation amounts cannot exceed `TotalAmount`. `DistributionSplitService.ValidateAllocationsSum()` validates exact equality.

**R3 — PAYOUT CONSISTENCY**
Payout must match distribution exactly. `PayoutAggregate.Initiate()` requires a non-empty `DistributionId`. `PayoutMatchingService.ValidatePayoutMatchesDistribution()` validates the link.

**R4 — NO DIRECT PAYMENT**
Revenue cannot directly trigger ledger entries. The flow is: Revenue → Distribution → Payout → Transaction → Journal → Entries. Any code path in the revenue context that creates journal entries or references ledger-level types is a **CRITICAL violation**.

**R5 — TRANSACTION REQUIRED**
Payout must go through the transaction context. Revenue payouts produce transactions, which produce journals, which produce entries. Payout does not write to ledger directly.

##### Revenue D-RULES (Domain Constraints)

**D46 — CONTRACT FOUNDATION**
All revenue must originate from a contract. `RevenueAggregate.Recognize()` enforces non-empty `ContractId`. `RevenueTraceService.ValidateOrigin()` validates the link. A revenue record without a contract reference is structurally invalid.

**D47 — DISTRIBUTION SUM RULE**
Allocations must equal total revenue. `DistributionAggregate.EnsureInvariants()` enforces sum of allocations <= total. `IsFullyAllocatedSpecification` validates exact equality. Over-allocation is an invariant violation.

**D48 — PAYOUT MATCH RULE**
Payout must match distribution. `PayoutAggregate` requires non-empty `DistributionId`. Payouts without a distribution reference are structurally invalid.

**D49 — NO DIRECT LEDGER WRITE**
Revenue cannot write to ledger directly. The revenue context (`src/domain/economic-system/revenue/`) MUST NOT contain any reference to `LedgerEntryAggregate`, `LedgerEntryRecordedEvent`, `JournalAggregate`, or ledger-level domain types. Revenue flows through transaction, not ledger.

**D50 — TRANSACTION FLOW REQUIRED**
Payout is intent-only at the domain boundary. The canonical flow is: `PayoutExecutedEvent` → (orchestration handles vault mutation in Phase 2D). Revenue context never directly interacts with the ledger or vault.

##### Revenue C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C31** | Revenue without contract reference | S0 — CRITICAL |
| **C32** | Distribution allocations do not match total revenue | S0 — CRITICAL |
| **C33** | Payout without distribution reference | S0 — CRITICAL |
| **C34** | Direct ledger write from revenue context | S0 — CRITICAL |
| **C35** | Payout without transaction linkage | S1 — HIGH |

##### Canonical Revenue Execution Order

```text
CANONICAL ECONOMIC FLOW (PHASE 2C)

RevenueRecordedEvent
  → records SPV revenue (external income)

DistributionCreatedEvent
  → computes ownership-based participant shares

PayoutExecutedEvent
  → emits payout intent (execution deferred to orchestration)

-------------------------------------

RULES:

1. Revenue is always normalized into SPV Slice1
2. Distribution uses allocation-defined ownership ratios
3. Payout is intent-only (no direct vault mutation in domain)
4. Vault debit/credit is executed via orchestration (Phase 2D)

-------------------------------------
```

##### Revenue Check Procedure

1. Scan `RevenueAggregate.Recognize()` — verify it requires non-empty `ContractId`.
2. Scan `DistributionAggregate.EnsureInvariants()` — verify allocation sum check.
3. Scan `PayoutAggregate.Initiate()` — verify it requires non-empty `DistributionId`.
4. Grep revenue context for `LedgerEntryAggregate`, `LedgerEntryRecordedEvent`, `JournalAggregate` — must find zero results.
5. Verify `PayoutAggregate` does not reference any ledger or journal types.
6. Verify `RevenueTraceService.ValidateOrigin()` checks contract reference.

##### Revenue Fail Criteria

- `RevenueAggregate` allows recognition without `ContractId` → **C31**
- `DistributionAggregate` allows finalization with mismatched allocations → **C32**
- `PayoutAggregate` allows initiation without `DistributionId` → **C33**
- Any file in `src/domain/economic-system/revenue/` references ledger types → **C34**
- Payout can complete without going through transaction context → **C35**

#### Exposure Context Rules (2026-04-10)

##### X-RULES (Cross-Domain Exposure)

**X1 — EXPOSURE SOURCE RULE**
All exposure must originate from one of: allocation, obligation, or transaction. `ExposureAggregate.Create()` requires a non-empty `SourceId` and a valid `ExposureType` (Allocation, Obligation, Transaction). Exposure without a source origin is a **CRITICAL violation**.

**X2 — NO ORPHAN EXPOSURE**
Exposure must always reference a valid economic source. An `ExposureAggregate` with an empty `SourceId` is an invariant violation. No exposure record may exist without a traceable link to a real economic action.

**X3 — EXPOSURE THRESHOLD RULE**
Exposure must not exceed defined limits. `ExposureThresholdSpecification.IsSatisfiedBy()` must be evaluated before any exposure increase is accepted. Exceeding the threshold without validation is a flow violation.

**X4 — EXPOSURE TRACEABILITY**
Exposure must be reconstructable from source events. The aggregate state is derived by replaying `ExposureCreatedEvent`, `ExposureIncreasedEvent`, `ExposureReducedEvent`, and `ExposureClosedEvent`. Any state not derivable from events is a violation.

**X5 — NON-MUTATIVE RULE**
Exposure does not change capital directly. The exposure context (`src/domain/economic-system/risk/exposure/`) MUST NOT contain any reference to `VaultAggregate`, `LedgerEntryAggregate`, `JournalAggregate`, `CapitalPoolAggregate`, or any capital/ledger domain types. Exposure reflects risk — it does not move funds.

##### Exposure D-RULES (Domain Constraints)

**D51 — EXPOSURE DOMAIN REQUIRED**
The `risk/exposure` domain must exist under `economic-system` with a complete DDD structure: aggregate/, entity/, error/, event/, service/, specification/, value-object/. Missing domain is a structural violation.

**D52 — SOURCE LINKAGE**
Exposure must reference allocation, obligation, or transaction. `ExposureAggregate.Create()` enforces non-empty `SourceId` and valid `ExposureType`. An exposure without a source reference is structurally invalid.

**D53 — NO ORPHAN EXPOSURE**
Exposure without a source is forbidden. `SourceId` is validated at creation via `Guard.Against(value == Guid.Empty)`. Any code path that creates an `ExposureAggregate` without a valid `SourceId` is a **CRITICAL violation**.

**D54 — THRESHOLD ENFORCEMENT**
Exposure must enforce limits. `ExposureThresholdSpecification.IsSatisfiedBy(currentExposure, threshold)` must be available and callable before exposure increases. Bypassing threshold validation is a flow violation.

**D55 — NON-MUTATIVE RULE**
Exposure cannot modify capital or ledger. The exposure context MUST NOT contain any reference to capital aggregates (`VaultAggregate`, `CapitalPoolAggregate`), ledger types (`LedgerEntryAggregate`, `JournalAggregate`), or transaction execution types. Exposure is read-only risk visibility.

##### Exposure C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C36** | Missing exposure domain (`risk/exposure` not found) | S0 — CRITICAL |
| **C37** | Exposure without source reference (`SourceId` empty) | S0 — CRITICAL |
| **C38** | Exposure exceeding threshold without validation | S1 — HIGH |
| **C39** | Orphan exposure detected (no traceable source) | S0 — CRITICAL |
| **C40** | Exposure modifying capital/ledger directly | S0 — CRITICAL |

##### Canonical Exposure Event Flow

```
1. Source event occurs              (DistributionCreatedEvent / ObligationCreatedEvent / TransactionInitiatedEvent)
2. Exposure created                 (ExposureCreatedEvent — X1, requires SourceId + ExposureType)
3. Exposure increased (optional)    (ExposureIncreasedEvent — X3, threshold must be checked)
4. Exposure reduced (optional)      (ExposureReducedEvent — amount <= current exposure)
5. Exposure closed                  (ExposureClosedEvent — zeroes exposure, sets Closed status)
```

Step 2 MUST include a valid SourceId and ExposureType (X1, D52). Step 3 MUST validate against threshold (X3, D54). Step 5 is terminal — no further mutations after Closed status.

##### Exposure Check Procedure

1. Verify `src/domain/economic-system/risk/exposure/` exists with all 7 mandatory subfolders — D51.
2. Scan `ExposureAggregate.Create()` — verify it requires non-empty `SourceId` and valid `ExposureType` — D52, D53.
3. Verify `ExposureThresholdSpecification.IsSatisfiedBy()` exists and accepts exposure + threshold — D54.
4. Grep exposure context for `VaultAggregate`, `CapitalPoolAggregate`, `LedgerEntryAggregate`, `JournalAggregate` — must find zero results — D55.
5. Verify `SourceId` constructor rejects `Guid.Empty` — D53.
6. Verify all aggregate state changes go through `RaiseDomainEvent` — X4.

##### Exposure Fail Criteria

- `risk/exposure` domain missing or incomplete → **C36**
- `ExposureAggregate` allows creation without `SourceId` → **C37**
- Exposure can increase without threshold validation path → **C38**
- `SourceId` accepts `Guid.Empty` → **C39**
- Any file in `src/domain/economic-system/risk/exposure/` references capital/ledger types → **C40**

#### Reconciliation Context Rules (2026-04-10)

##### RC-RULES (Reconciliation Flow)

**RC1 — LEDGER AUTHORITATIVE**
Reconciliation validates alignment between the ledger (source of truth) and observed/derived states. Reconciliation MUST NOT modify ledger state. The reconciliation context (`src/domain/economic-system/reconciliation/`) MUST NOT contain any reference to `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, or any write-side ledger types.

**RC2 — RESULT REQUIRED**
Every reconciliation process must produce a result. `ProcessAggregate` must transition to either `Matched` or `Mismatched` before it can be resolved. Calling `Resolve()` on a process in `Pending` or `InProgress` status is a flow violation.

**RC3 — DISCREPANCY TRACEABILITY**
Every discrepancy must reference a reconciliation process. `DiscrepancyAggregate.Detect()` requires a non-empty `ProcessReference`. A discrepancy without a process origin is a **CRITICAL violation**.

**RC4 — NO IGNORED DISCREPANCY**
Discrepancies must not be silently dropped. Every detected discrepancy must be either acknowledged or resolved. The system must not permit deletion or silent removal of discrepancy records.

**RC5 — NON-MUTATIVE**
Reconciliation does not create or modify financial truth. The reconciliation context reads and compares — it never writes to capital, ledger, or transaction contexts.

##### Reconciliation D-RULES (Domain Constraints)

**D56 — RECONCILIATION DOMAINS REQUIRED**
The `reconciliation` context must contain two domains: `process` and `discrepancy`. Each domain must have all 7 mandatory DDD subfolders. Missing domain is a structural violation.

**D57 — PROCESS MUST PRODUCE RESULT**
`ProcessAggregate.Resolve()` enforces that status is `Matched` or `Mismatched` before resolution. Resolving a `Pending` or `InProgress` process is forbidden.

**D58 — DISCREPANCY REQUIRES PROCESS**
`DiscrepancyAggregate.Detect()` enforces non-empty `ProcessReference`. A discrepancy without a process reference is structurally invalid.

**D59 — NO LEDGER MUTATION**
The reconciliation context MUST NOT reference write-side ledger types (`LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`). Reconciliation is read-only comparison.

**D60 — DISCREPANCY LIFECYCLE**
Discrepancies follow the lifecycle: Open → Investigating → Resolved. `Investigate()` requires `Open` status. `Resolve()` requires `Open` or `Investigating` status. Resolved discrepancies are terminal.

**D61 — COMPARISON DATA REQUIRED**
`DiscrepancyAggregate.Detect()` requires `ExpectedValue` (from ledger) and `ActualValue` (observed). The `Difference` is computed as `ExpectedValue - ActualValue`. A discrepancy without comparison data is a **CRITICAL violation**.

**D62 — DISCREPANCY SOURCE REQUIRED**
Every discrepancy must declare its source: `Projection` or `ExternalSystem`. The `DiscrepancySource` enum is mandatory at detection time.

##### Reconciliation C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C41** | Missing reconciliation domain (`process` or `discrepancy` not found) | S0 — CRITICAL |
| **C42** | Reconciliation resolved without result (Pending/InProgress → Resolved) | S0 — CRITICAL |
| **C43** | Discrepancy without process reference | S0 — CRITICAL |
| **C44** | Reconciliation context modifying ledger state | S0 — CRITICAL |
| **C45** | Discrepancy silently dropped or deleted | S1 — HIGH |
| **C46** | Discrepancy without comparison data (ExpectedValue/ActualValue) | S0 — CRITICAL |
| **C47** | Discrepancy without source declaration | S0 — CRITICAL |

##### Canonical Reconciliation Event Flow

```
1. ReconciliationTriggeredEvent      → Process initiated with ledger + observed references
2. [Comparison logic]                → Engine/runtime performs comparison
3a. ReconciliationMatchedEvent       → States align (RC2 — result produced)
3b. ReconciliationMismatchedEvent    → States diverge (RC2 — result produced)
4. DiscrepancyDetectedEvent (if 3b)  → Mismatch tracked (RC3 — requires ProcessReference, ExpectedValue, ActualValue)
5. DiscrepancyInvestigatedEvent      → Mismatch under investigation (Open → Investigating)
6. DiscrepancyResolvedEvent          → Mismatch resolved with resolution description
7. ReconciliationResolvedEvent       → Process finalized (RC2 — requires result first)
```

Step 3 MUST precede step 7. Step 4 MUST reference step 1. Step 6 MUST include a resolution description.

##### Reconciliation Check Procedure

1. Verify `src/domain/economic-system/reconciliation/process/` and `src/domain/economic-system/reconciliation/discrepancy/` exist with all 7 mandatory subfolders — D56.
2. Scan `ProcessAggregate.Resolve()` — verify it rejects `Pending` and `InProgress` status — D57.
3. Scan `DiscrepancyAggregate.Detect()` — verify it requires non-empty `ProcessReference` — D58.
4. Grep reconciliation context for `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate` — must find zero results — D59.
5. Verify `DiscrepancyAggregate` state transitions follow Open → Investigating → Resolved — D60.
6. Verify `ProcessReference` constructor rejects `Guid.Empty` — D58.
7. Verify `DiscrepancyAggregate.Detect()` requires `ExpectedValue` and `ActualValue` parameters and computes `Difference` — D61.
8. Verify `DiscrepancyAggregate.Detect()` requires `DiscrepancySource` parameter — D62.

##### Reconciliation Fail Criteria

- `reconciliation/process` or `reconciliation/discrepancy` domain missing → **C41**
- `ProcessAggregate` allows resolution without prior result → **C42**
- `DiscrepancyAggregate` allows detection without `ProcessReference` → **C43**
- Any file in `src/domain/economic-system/reconciliation/` references write-side ledger types → **C44**
- Discrepancy records can be deleted or dropped without resolution → **C45**
- `DiscrepancyAggregate` allows detection without `ExpectedValue`/`ActualValue` → **C46**
- `DiscrepancyAggregate` allows detection without `DiscrepancySource` → **C47**

#### Cross-Domain Reconciliation Rules (2026-04-10)

These rules apply across all domain boundaries that interact with reconciliation. They reinforce and extend the context-specific RC-RULES above.

##### CR-RULES (Cross-Domain)

**CR1 — LEDGER AUTHORITY**
Ledger is the single source of truth. Reconciliation always validates observed state against ledger state — never the reverse. Reinforces RC1.

**CR2 — NO MUTATION**
Reconciliation cannot modify ledger, capital, or transaction state. This extends RC5 to explicitly cover capital and transaction contexts in addition to ledger.

**CR3 — DISCREPANCY REQUIRED**
All mismatches between ledger and observed state must produce a discrepancy record. Silent discarding of mismatches is forbidden. Reinforces RC4.

**CR4 — TRACEABILITY**
Each discrepancy must be traceable to:
- **Ledger source** — the expected value from the authoritative ledger
- **Observed source** — the actual value from the projection or external system
Reinforces D61, D62.

**CR5 — PROCESS COMPLETENESS**
Every reconciliation must terminate in either `Completed` (all matched or all discrepancies resolved) or `Failed` (unresolvable). No reconciliation may remain in `Pending` or `InProgress` indefinitely. Reinforces D57.

##### Cross-Domain D-RULES (Guard Extension)

**D63 — RECONCILIATION DOMAIN REQUIRED**
`reconciliation/process` and `reconciliation/discrepancy` must exist as fully structured DDD domains. Extends D56.

**D64 — LEDGER AS SOURCE**
Reconciliation must always reference ledger as the authoritative truth source. The `ExpectedValue` in discrepancy detection must originate from ledger data. Extends CR1.

**D65 — DISCREPANCY TRACKING**
All mismatches must be recorded as `DiscrepancyAggregate` instances. No mismatch may be silently ignored. Extends CR3, RC4.

##### Cross-Domain C-CONSTRAINTS (Audit Extension)

| Code | Violation | Severity |
|---|---|---|
| **C48** | Ledger not used as authoritative source in reconciliation | S0 — CRITICAL |
| **C49** | Financial mutation detected in reconciliation context (capital/transaction/revenue) | S0 — CRITICAL |
| **C50** | Incomplete reconciliation process (stuck in Pending/InProgress) | S1 — HIGH |

#### Enforcement & Compliance Cross-Domain Rules (2026-04-10)

These rules apply across the enforcement and compliance control layers. They govern rule definition, violation tracking, audit evidence, and the boundary between control and financial truth.

##### E-RULES (Cross-Domain)

**E1 — RULE FOUNDATION**
Every violation must reference a defined enforcement rule. `ViolationAggregate.Detect()` requires a non-empty `RuleId`. `RuleAggregate` must exist and be identifiable before a violation can reference it. Violations without rule foundation are **CRITICAL violations**.

**E2 — SOURCE TRACEABILITY**
Violations and audit records must reference source domain, source aggregate, and source event where available. `ViolationAggregate.Detect()` requires a non-empty `SourceReference`. `AuditRecordAggregate.CreateRecord()` requires `SourceDomain`, `SourceAggregateId`, and `SourceEventId`. Traceability must be reconstructable from stored references.

**E3 — NO FINANCIAL MUTATION**
Enforcement and compliance cannot modify capital, ledger, transaction, revenue, or settlement truth. The enforcement context (`src/domain/economic-system/enforcement/`) and compliance context (`src/domain/economic-system/compliance/`) MUST NOT contain any reference to `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, `CapitalPoolAggregate`, `TransactionAggregate`, or any write-side financial types. Control layers observe and record — they never modify.

**E4 — TERMINAL RESOLUTION**
Resolved, dismissed, and finalized states are terminal. `ViolationAggregate` in `Resolved` status accepts no further state transitions. `AuditRecordAggregate` in `Finalized` status accepts no further mutations. Any attempt to reopen or mutate a terminal record is a **CRITICAL violation**.

**E5 — REVIEWABILITY**
All violations and audit records must contain enough context for human and system review. `ViolationAggregate.Detect()` requires a non-empty `reason`. `AuditRecordAggregate.CreateRecord()` requires a non-empty `EvidenceSummary`. Records without review context are flow violations.

**E6 — IMMUTABLE EVIDENCE**
Finalized audit records are immutable. `AuditRecordAggregate.FinalizeRecord()` transitions status to `Finalized`. Any mutation attempt on a finalized record must be rejected. This is enforced by `Guard.Against(Status == AuditRecordStatus.Finalized)` in `FinalizeRecord()`.

**E7 — UNIQUE RULE IDENTITY**
`RuleCode` must be unique and stable across the system. `RuleId` constructor rejects `Guid.Empty`. Every rule must have a non-empty `Name` and `Description`. Rules are identified by their `RuleId` and must not be duplicated.

**E8 — VIOLATION WITHOUT RULE IS FORBIDDEN**
No ad hoc violation records. Every `ViolationAggregate` must reference an existing `RuleId`. `ViolationAggregate.EnsureInvariants()` enforces that `RuleId` is non-empty. Orphan violations (missing rule reference) are invariant violations.

**E9 — AUDIT WITHOUT SOURCE IS FORBIDDEN**
No orphan audit records. Every `AuditRecordAggregate` must reference a source via `SourceDomain`, `SourceAggregateId`, and `SourceEventId`. Audit records without traceable source references are structurally invalid.

**E10 — COMPLIANCE RECORDS EVIDENCE, NOT POLICY**
Compliance stores evidence of what happened, not policy evaluation logic. The compliance context contains audit records that capture facts. Policy decisions, rule evaluation, and enforcement logic belong in the enforcement context or governance layer — not in compliance.

##### Enforcement & Compliance D-RULES (Guard Extension)

**D66 — ENFORCEMENT DOMAINS REQUIRED**
`economic-system/enforcement/rule` and `economic-system/enforcement/violation` must exist as fully structured DDD domains with all 7 mandatory subfolders (aggregate, entity, error, event, service, specification, value-object). Missing domain is a structural violation.

**D67 — COMPLIANCE DOMAIN REQUIRED**
`economic-system/compliance/audit` must exist as a fully structured DDD domain with all 7 mandatory subfolders. Missing domain is a structural violation.

**D68 — RULE REFERENCE REQUIRED**
Violations must reference a valid rule. `ViolationAggregate.Detect()` enforces non-empty `RuleId`. `ViolationAggregate.EnsureInvariants()` rejects empty `RuleId`. Extends E1, E8.

**D69 — SOURCE REFERENCE REQUIRED**
Violations and audit records must reference a source. `ViolationAggregate.Detect()` enforces non-empty `SourceReference`. `AuditRecordAggregate.CreateRecord()` enforces non-empty `SourceDomain`, `SourceAggregateId`, and `SourceEventId`. Extends E2, E9.

**D70 — NO FINANCIAL MUTATION IN CONTROL LAYERS**
Enforcement and compliance cannot modify financial state. `src/domain/economic-system/enforcement/` and `src/domain/economic-system/compliance/` MUST NOT reference `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, `CapitalPoolAggregate`, `TransactionAggregate`. Extends E3.

**D71 — FINALIZED IMMUTABILITY**
Finalized audit records are immutable. `AuditRecordAggregate.FinalizeRecord()` rejects `Finalized` status. No further mutations after finalization. Extends E6.

**D72 — TERMINAL STATE ENFORCEMENT**
Resolved violations and finalized audit records are terminal. `ViolationAggregate.Resolve()` requires `Acknowledged` status — resolved violations accept no further transitions. `AuditRecordAggregate.FinalizeRecord()` requires `Draft` status — finalized records accept no further mutations. Extends E4.

**D73 — RULE CODE UNIQUENESS**
`RuleCode` / `RuleId` must be unique. `RuleId` constructor rejects `Guid.Empty`. No two rules may share the same identifier. Extends E7.

**D74 — EVIDENCE COMPLETENESS**
Audit records must include non-empty evidence summary and review context. `AuditRecordAggregate.CreateRecord()` requires a non-empty `EvidenceSummary`. Violations must include a non-empty `reason`. Extends E5.

##### Enforcement & Compliance C-CONSTRAINTS (Violation Codes)

| Code | Violation | Severity |
|---|---|---|
| **C51** | Missing enforcement domains (`rule` or `violation` not found) | S0 — CRITICAL |
| **C52** | Missing compliance domain (`audit` not found) | S0 — CRITICAL |
| **C53** | Violation without rule reference (`RuleId` empty) | S0 — CRITICAL |
| **C54** | Violation without source reference (`SourceReference` empty) | S0 — CRITICAL |
| **C55** | Audit record without source (`SourceDomain`/`SourceAggregateId`/`SourceEventId` empty) | S0 — CRITICAL |
| **C56** | Financial mutation inside enforcement or compliance context | S0 — CRITICAL |
| **C57** | Finalized audit record mutated | S0 — CRITICAL |
| **C58** | Duplicate rule code / non-unique `RuleId` | S1 — HIGH |
| **C59** | Terminal state reopened (resolved violation or finalized audit record mutated) | S0 — CRITICAL |
| **C60** | Empty evidence summary or missing violation reason | S1 — HIGH |

##### Canonical Enforcement & Compliance Event Flow

```
Enforcement:
1. RuleDefinedEvent               → Rule created with name, description, scope (E7)
2. RuleEvaluatedEvent             → Rule evaluated against subject, pass/fail (E1)
3. ViolationDetectedEvent         → Breach recorded (E1 — RuleId, E2 — Source, E5 — Reason)
4. ViolationAcknowledgedEvent     → Violation acknowledged (E4 — requires Open status)
5. ViolationResolvedEvent         → Violation resolved (E4 — terminal, requires Acknowledged + resolution)

Compliance:
1. Source action occurs            (domain event from any economic context)
2. AuditRecordCreatedEvent        → Evidence captured (E2 — SourceDomain + SourceAggregateId + SourceEventId, E5 — EvidenceSummary)
3. AuditRecordFinalizedEvent      → Evidence sealed (E6 — immutable after this point, E4 — terminal)
```

Enforcement step 3 MUST include RuleId, SourceReference, and Reason. Step 5 is terminal. Compliance step 2 MUST include all source references and evidence summary. Step 3 is terminal.

##### Enforcement & Compliance Check Procedure

1. Verify `src/domain/economic-system/enforcement/rule/` and `src/domain/economic-system/enforcement/violation/` exist with all 7 mandatory subfolders — D66.
2. Verify `src/domain/economic-system/compliance/audit/` exists with all 7 mandatory subfolders — D67.
3. Scan `ViolationAggregate.Detect()` — verify it requires non-empty `RuleId` — D68.
4. Scan `ViolationAggregate.Detect()` — verify it requires non-empty `SourceReference` — D69.
5. Scan `ViolationAggregate.Detect()` — verify it requires non-empty `reason` — D74.
6. Scan `AuditRecordAggregate.CreateRecord()` — verify it requires `SourceDomain`, `SourceAggregateId`, `SourceEventId`, and `EvidenceSummary` — D69, D74.
7. Scan `AuditRecordAggregate.FinalizeRecord()` — verify it rejects `Finalized` status — D71.
8. Grep enforcement and compliance contexts for `LedgerEntryAggregate`, `JournalAggregate`, `VaultAggregate`, `CapitalPoolAggregate`, `TransactionAggregate` — must find zero results — D70.
9. Verify `ViolationAggregate.EnsureInvariants()` rejects empty `RuleId` and `Source` — D68.
10. Verify `RuleId`, `ViolationId`, `SourceReference` constructors reject `Guid.Empty` — D68, D69.
11. Verify `ViolationAggregate.Resolve()` requires `Acknowledged` status — D72.
12. Verify `AuditRecordAggregate` accepts no mutations after `Finalized` — D71, D72.

##### Enforcement & Compliance Fail Criteria

- `enforcement/rule` or `enforcement/violation` domain missing → **C51**
- `compliance/audit` domain missing → **C52**
- `ViolationAggregate` allows detection without `RuleId` → **C53**
- `ViolationAggregate` allows detection without `SourceReference` → **C54**
- `AuditRecordAggregate` allows creation without source references → **C55**
- Any file in enforcement or compliance references write-side financial types → **C56**
- `AuditRecordAggregate` allows mutation after finalization → **C57**
- `RuleId` accepts `Guid.Empty` or duplicate identifiers → **C58**
- Resolved violation or finalized audit record accepts further state changes → **C59**
- `ViolationAggregate` allows detection without reason, or `AuditRecordAggregate` allows creation without `EvidenceSummary` → **C60**

#### Cross-Domain Exchange Rules (2026-04-10)

These rules apply across all domain boundaries that interact with exchange rates and routing. They govern FX operations, rate integrity, and transaction routing.

##### Exchange X-RULES (Cross-Domain)

**X1 — RATE REQUIRED**
All FX operations must reference a rate. `FxAggregate` operations that involve currency conversion MUST include a non-empty `RateId` referencing a valid `ExchangeRateAggregate`. FX without a rate reference is a **CRITICAL violation**.

**X2 — RATE IMMUTABILITY**
Active rates cannot be modified. An `ExchangeRateAggregate` in `Active` status accepts only the `Expire()` transition — no value changes, no currency changes, no version changes. Any mutation attempt on an active rate is a **CRITICAL violation**.

**X3 — ROUTING REQUIRED**
All transactions must go through routing. Transaction execution that involves cross-currency or multi-path resolution MUST pass through the routing context. Bypassing routing is a flow violation.

**X4 — DETERMINISTIC ROUTING**
Routing decisions must be reproducible. Given the same inputs (source, destination, amount, rate), routing MUST produce the same path selection. Non-deterministic routing (e.g., random path selection, system-time-based decisions) violates §9 determinism rules.

**X5 — PATH VALIDATION**
Only active paths can be selected. Routing MUST validate that selected paths are in `Active` status before execution. Selecting an inactive, suspended, or expired path is a flow violation.

##### Exchange D-RULES (Guard Extension)

**D75 — EXCHANGE RATE DOMAIN REQUIRED**
`exchange/rate` must exist as a fully structured DDD domain under `economic-system` with all 7 mandatory subfolders (aggregate, entity, error, event, service, specification, value-object). Missing domain is a structural violation.

**D76 — FX MUST USE RATE**
FX operations must reference a rate. Any currency conversion operation in the `exchange/fx` context MUST include a `RateId` parameter linking to an `ExchangeRateAggregate`. FX without rate reference is structurally invalid.

**D77 — ROUTING PATH REQUIRED**
`exchange/routing` (or equivalent routing domain) must exist under `economic-system`. The routing domain provides path resolution for cross-currency and multi-hop transactions. Missing routing domain is a structural violation.

**D78 — ROUTING MANDATORY**
Transactions that cross currency boundaries or require path resolution MUST pass through routing. Direct transaction execution that bypasses routing is a flow violation.

**D79 — RATE IMMUTABILITY**
Active rates cannot be modified. `ExchangeRateAggregate` in `Active` status only accepts `Expire()`. No property mutation is permitted on active rates. This is enforced by the aggregate's state transition rules: `Active` → `Expired` only.

##### Exchange C-CONSTRAINTS (Audit Extension)

| Code | Violation | Severity |
|---|---|---|
| **C61** | Missing rate domain (`exchange/rate` not found or incomplete) | S0 — CRITICAL |
| **C62** | FX operation without rate reference (`RateId` empty or missing) | S0 — CRITICAL |
| **C63** | Missing routing path domain (`exchange/routing` not found) | S1 — HIGH |
| **C64** | Transaction bypassing routing (direct execution without path resolution) | S1 — HIGH |
| **C65** | Mutable active rate (Active rate modified instead of expired and replaced) | S0 — CRITICAL |

##### Canonical Exchange Event Flow

```
1. ExchangeRateDefinedEvent         → Rate created with currency pair, value, version (X1)
2. ExchangeRateActivatedEvent       → Rate becomes active (X2 — immutable after this)
3. [FX operation references rate]   → FX must include RateId (X1, D76)
4. [Routing resolves path]          → Path selected deterministically (X3, X4, X5)
5. [Transaction executes via path]  → Transaction flows through routing (X3, D78)
6. ExchangeRateExpiredEvent         → Rate retired (X2 — only valid transition from Active)
```

Step 2 makes the rate immutable (X2, D79). Step 3 MUST reference an active rate (X1, D76). Step 4 MUST select active paths only (X5). Step 5 MUST go through routing (X3, D78).

##### Exchange Check Procedure

1. Verify `src/domain/economic-system/exchange/rate/` exists with all 7 mandatory subfolders — D75.
2. Verify `src/domain/economic-system/exchange/routing/` exists (or equivalent routing domain) — D77.
3. Scan `ExchangeRateAggregate` — verify Active status only permits `Expire()` — D79.
4. Scan FX context for rate references — verify FX operations require `RateId` — D76.
5. Verify routing path selection validates `Active` status — X5.
6. Verify routing produces deterministic output for identical inputs — X4.

##### Exchange Fail Criteria

- `exchange/rate` domain missing or incomplete → **C61**
- FX operation in `exchange/fx` allows execution without `RateId` → **C62**
- `exchange/routing` domain missing → **C63**
- Transaction can execute without passing through routing → **C64**
- `ExchangeRateAggregate` in `Active` status accepts mutations beyond `Expire()` → **C65**

### Governance

Doctrinal guard for governance/policy flow across the domain.

#### Governance Rules (2026-04-07 Normalization)

##### RULE: POLICY-ENFORCEMENT-01 — POLICY MUST GATE EXECUTION
Policy MUST execute BEFORE any domain or engine execution.

ENFORCEMENT:
- PolicyMiddleware must run BEFORE aggregate load
- Deny MUST terminate execution immediately
- No bypass allowed from workflow or runtime

##### RULE: POLICY-CHAIN-01 — POLICY DECISION MUST BE ANCHORED
Every policy decision MUST be written to WhyceChain.

ENFORCEMENT:
- DecisionHash must be generated deterministically
- Chain anchoring must occur BEFORE Kafka publish

##### RULE: POLICY-DETERMINISM-01 — POLICY DECISION DETERMINISTIC
Policy evaluation MUST produce deterministic outputs.

ENFORCEMENT:
- No timestamp/random input into policy hashing
- Same input MUST produce same DecisionHash

### Identity

Doctrinal guard for identity context resolution.

#### Identity Rules (2026-04-07 Normalization)

##### RULE: ID-POLICY-01 — IDENTITY MUST BE POLICY-RESOLVED
All identity context MUST be resolved BEFORE policy evaluation.

ENFORCEMENT:
- IdentityMiddleware MUST run before PolicyMiddleware
- Identity context must include: subjectId, roles, trustScore, verificationStatus

##### RULE: ID-DETERMINISM-01 — IDENTITY DETERMINISM
Identity resolution MUST be deterministic across replay.

ENFORCEMENT:
- No random ID/session generation in runtime flow
- Identity must be derived from request context deterministically

### Observability

Doctrinal guard for traceability and replay determinism at the domain boundary.

#### Observability Rules (2026-04-07 Normalization)

##### RULE: OBS-TRACE-01 — FULL PIPELINE TRACEABILITY
All executions MUST emit traceable metadata.

ENFORCEMENT:
- Must include: EventId, ExecutionHash, DecisionHash, PolicyVersion

##### RULE: OBS-REPLAY-01 — REPLAY DETERMINISM SUPPORT
System MUST support deterministic replay mode.

ENFORCEMENT:
- Replay must reproduce identical: events, hashes, decisions

### Workflow

Doctrinal guard for workflow placement, classification, and pipeline discipline.

#### Workflow Rules (2026-04-07 Normalization)

##### RULE: WF-PLACEMENT-01 — WORKFLOW EXECUTION LAYER LOCK
Workflow execution MUST occur ONLY in T1M engines.

ENFORCEMENT:
- systems layer MUST be declarative only
- runtime MUST NOT implement workflow logic
- engines/T1M is the ONLY execution layer

##### RULE: WF-TYPE-01 — WORKFLOW CLASSIFICATION
Only TWO workflow types are allowed:

1. Operational Workflow (short-term execution)
2. Lifecycle Workflow (state transition orchestration)

ENFORCEMENT:
- All workflows must be classified explicitly
- No hybrid undefined workflows

##### RULE: WF-PIPELINE-01 — WORKFLOW MUST PASS FULL PIPELINE
All workflow execution MUST pass through runtime control plane.

ENFORCEMENT:
- WorkflowStartCommand MUST go through: Guard → Policy → Idempotency → Execution

---

## WBSM v3 Global Enforcement

The following WBSM v3 global enforcement rules apply uniformly to the domain layer and appear in every domain-bearing guard. They are restated once here as the canonical form.

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

## Unified Check Procedure

The following procedure covers the full domain purity + structure + behavior surface. Per-subsection procedures above (Transaction, Revenue, Exposure, Reconciliation, Enforcement & Compliance, Exchange, DTO, Structural, Behavioral) remain authoritative for their respective scopes and MUST also be run.

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

## Unified Pass Criteria

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

## Unified Fail Criteria

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

## Unified Domain Severity Levels

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

## Source Consolidation Ledger

This canonical guard absorbs the following source files:

1. `claude/guards/domain.guard.md` (self — prior content preserved in §Domain Layer Purity)
2. `claude/guards/domain-structure.guard.md` (§Domain Structure)
3. `claude/guards/classification-suffix.guard.md` (§Classification Suffix Conventions)
4. `claude/guards/dto-naming.guard.md` (§DTO Naming)
5. `claude/guards/behavioral.guard.md` (§Behavioral Rules)
6. `claude/guards/structural.guard.md` (§Structural Rules)
7. `claude/guards/domain-aligned/economic.guard.md` (§Domain-Aligned > Economic, all 621 lines preserved verbatim)
8. `claude/guards/domain-aligned/governance.guard.md` (§Domain-Aligned > Governance)
9. `claude/guards/domain-aligned/identity.guard.md` (§Domain-Aligned > Identity)
10. `claude/guards/domain-aligned/observability.guard.md` (§Domain-Aligned > Observability)
11. `claude/guards/domain-aligned/workflow.guard.md` (§Domain-Aligned > Workflow)

**NOT absorbed here (relocated to `runtime.guard.md` per 2026-04-14 layer-doctrine canonicalization):**
- `claude/guards/contracts-boundary.guard.md` → §Contracts Boundary (runtime — enforcement boundary, not business truth)
- `claude/guards/dependency-graph.guard.md` → §Dependency Graph & Layer Boundaries (runtime — enforcement/dependency rules)

### Dedups Performed

- **WBSM v3 GE-01..GE-05 Global Enforcement block** appears identically in `domain.guard.md` and `behavioral.guard.md`. Preserved once in §WBSM v3 Global Enforcement.
- **Determinism prohibition** (`Guid.NewGuid`, `DateTime.Now/UtcNow`, `Random`) appears in: §Domain Purity rule 12, §Domain Purity Extensions D-DET-01/D-NO-SYSCLOCK, §Behavioral Rules rule 16, §Behavioral Extensions B-CLOCK-01/B-ID-01, GE-01. All preserved because each binds distinct scopes (domain-only vs engines/runtime/domain vs platform/host/adapters vs aggregate ID factories).
- **Event-first / every state change emits event** appears in §Domain Purity rule 23, §Behavioral Rules rule 15, GE-04, and ECON-ES-01. All four preserved — scopes differ (all domain vs runtime-routed events vs global constraint vs economic context specifically).
- **Structural layer-purity rules** (§Structural Rules) express layer obligations from the *domain* viewpoint (what domain MUST NOT do). The mechanical CI enforcement (DG-R1..DG-R7, DG-R5-EXCEPT-01, DG-R5-HOST-DOMAIN-FORBIDDEN, `scripts/dependency-check.sh`) lives in `runtime.guard.md` §Dependency Graph & Layer Boundaries. These surfaces reinforce one another without textual duplication.

### Semantic Conflicts (Reported, Not Silently Resolved)

1. **Severity conflict on `-system` suffix rule.** `DS-R1`/`DS-R2` (domain-structure) list this as **S1**. `CLASS-SFX-R1`/`CLASS-SFX-R2` (classification-suffix) list this as **S0 build-blocking**. Both preserved verbatim. Audit tooling SHOULD treat as S0 (the stricter) until canonical resolution.
2. **DG-R5 platform permission on domain** (cross-reference only; authoritative rule lives in `runtime.guard.md` §Dependency Graph). Prior `platform.guard.md` G-PLATFORM-07 permitted `platform/host → domain` for DI registration; `dependency-graph.guard.md` DG-R5-HOST-DOMAIN-FORBIDDEN (2026-04-08) strengthens this to a prohibition. The strengthened rule is authoritative — see `runtime.guard.md` §Dependency Graph & Layer Boundaries.
3. **Workflow placement vs engine isolation.** `WF-PLACEMENT-01` mandates workflow execution in T1M engines while `DG-R2` forbids engines from referencing runtime. These are compatible (engines own workflow execution; runtime orchestrates around them), but the interaction warrants audit attention.
4. **`X-RULES` prefix collision in economic guard** between Exposure (section earlier in the source) and Exchange (section later). The canonical text has been preserved verbatim; audit procedures should disambiguate by section context (Exposure X1..X5 vs Exchange X1..X5 are distinct rule sets).

---

## Rules Promoted from new-rules/ (2026-04-18)

Rules below were captured in `claude/new-rules/` per CLAUDE.md $1c and promoted into this guard on 2026-04-18. Rule IDs are indexed in `claude/audits/domain.audit.md`.

### RUNBOOK-CONTROL-PLANE-COVERAGE-01 — Runbook Sync on Route Changes

Definition:
When a public route is restricted or removed, the same PR MUST update every operator runbook (`*.md` under repo root, `docs/`, and project-topic/audit references) that names the route. Static check: grep changed routes against runbook files in CI; warn on uncovered references.

Enforcement:
CI job: diff the PR's route changes against all `*.md` files under `docs/**` and `claude/project-topics/**`; any uncovered route string reference = warn (S2 fail once stabilised).

Severity:
S2

References:
- CLAUDE.md $1b (post-execution audit sweep)
- Source: `claude/new-rules/20260417-122320-economic-system-phase4-5-final-residual.md`

### DG-R-OUT-EFF-PLACEMENT-01 — Outbound-Effect Aggregate Placement

Definition:
`OutboundEffectAggregate` and its eleven canonical lifecycle events
(`OutboundEffectScheduledEvent`, `OutboundEffectDispatchedEvent`,
`OutboundEffectAcknowledgedEvent`, `OutboundEffectDispatchFailedEvent`,
`OutboundEffectRetryAttemptedEvent`, `OutboundEffectRetryExhaustedEvent`,
`OutboundEffectFinalizedEvent`, `OutboundEffectReconciliationRequiredEvent`,
`OutboundEffectReconciledEvent`, `OutboundEffectCompensationRequestedEvent`,
`OutboundEffectCancelledEvent`) MUST live under
`src/domain/integration-system/outbound-effect/**` per the three-level
nesting `classification/context/domain` (D-R3B-1 ratified 2026-04-20,
new top-level `integration-system` classification). Placement elsewhere —
including under `business-system/integration/**` or any other
classification — is S1 drift.

Enforcement:
Architecture test `OutboundEffect_Aggregate_And_Events_Under_IntegrationSystem`
scans for `OutboundEffectAggregate` / `OutboundEffect*Event` type definitions
outside the canonical path.

Severity:
S1

References:
- `src/domain/integration-system/outbound-effect/aggregate/OutboundEffectAggregate.cs`
- `src/domain/integration-system/outbound-effect/event/**`
- `claude/project-topics/v2b/closure/20260420-100946-r3-b-design.md` §3.2, §17.1 D-R3B-1
- `claude/project-topics/v2b/closure/20260420-103811-r3-b-1-design.md` §3.12
