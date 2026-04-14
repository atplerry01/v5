# Infrastructure Guard (Canonical)

## Purpose

Consolidated governance for the platform + systems edge of WBSM v3. This guard enforces:

- Platform layer boundaries — strict separation between `src/platform/api` (external interface) and `src/platform/host` (application host / composition root), and their relationships to runtime/engines/infrastructure.
- Systems integration — `src/systems/` as a pure composition layer (upstream/midstream/downstream) that wires engines via runtime without executing domain logic, mutating state, or persisting directly.
- Kafka event fabric — dual-topic pattern, canonical topic naming, outbox pattern, dead-letter routing, headers, partition determinism, and schema governance.
- Config & secret safety — no credentials, no hardcoded endpoints, no hardcoded fallbacks, strongly-typed options, docker-compose scanning.

## Source consolidation

Absorbs: `platform.guard.md`, `systems.guard.md`, `kafka.guard.md`, `config-safety.guard.md`, `composition-loader.guard.md`, `program-composition.guard.md`.

Note on rule-ID preservation: platform rules are cross-filed as `R-PLAT-XX` with their original `G-PLATFORM-XX` / `PLAT-*-01` IDs retained in each heading. Kafka rules are cross-filed as `R-K-XX` with original `K-XX` / `K-TOPIC-*-01` / `K-DLQ-*-01` / `K-HEADER-CONTRACT-01` / `K-OUTBOX-ISOLATION-01` / `K-DETERMINISTIC-01` IDs preserved in each heading. Composition rules (`G-COMPLOAD-01..07`, `G-PROGCOMP-01..05`) retain their original IDs verbatim in the Composition Loader and Program Composition sections below.

### Previous consolidation notes

This guard supersedes and merges the following previously-separate guard files:

- `claude/guards/platform.guard.md` — Platform API / Host boundary enforcement.
- `claude/guards/systems.guard.md` — Systems composition layer rules.
- `claude/guards/kafka.guard.md` — Kafka governance rules.
- `claude/guards/config-safety.guard.md` — Config and secret safety rules.

All rules from the four source files are preserved. Deduplications are limited to exact semantic overlaps; see the Dedup Notes section at the end.

## Scope

- Platform Boundaries: all files under `src/platform/` (both `api/` and `host/`).
- Systems Integration: all files under `src/systems/`.
- Kafka Event Fabric: all files across `src/` and `infrastructure/` that reference Kafka, message brokers, or event streaming.
- Config & Secret Safety: all files under `src/`, `infrastructure/`; test fixtures under `tests/` are out of scope unless they hit production-shared files.

Evaluated at CI, code review, and architectural audit.

---

## Rules

### Section: Platform Boundaries

Source: `platform.guard.md`.

#### R-PLAT-01 — G-PLATFORM-01: API Layer Purity
**Source:** platform.guard.md (G-PLATFORM-01)

`/src/platform/api/` MUST contain ONLY:
- Controllers (`[ApiController]`)
- DTOs and request/response types
- Swagger/OpenAPI configuration
- Health aggregation (composition of health check results)

#### R-PLAT-02 — G-PLATFORM-02: Host Layer Purity
**Source:** platform.guard.md (G-PLATFORM-02)

`/src/platform/host/` MUST contain ONLY:
- DI container setup (`builder.Services.*`)
- Runtime wiring and middleware registration
- Infrastructure adapter implementations (health checks, in-memory stores)
- Application bootstrap (`Program.cs`)

#### R-PLAT-03 — G-PLATFORM-03: No Controllers in Host
**Source:** platform.guard.md (G-PLATFORM-03)

BLOCK if any file in `/src/platform/host/` contains:
- `[ApiController]` attribute
- `MapGet`, `MapPost`, `MapPut`, `MapDelete`, `MapPatch` calls that define business endpoints

#### R-PLAT-04 — G-PLATFORM-04: No DI in API
**Source:** platform.guard.md (G-PLATFORM-04)

BLOCK if any file in `/src/platform/api/` contains:
- `IServiceCollection` extensions that register infrastructure adapters
- Direct `builder.Services.*` calls (except swagger/controller registration in extension methods)

#### R-PLAT-05 — G-PLATFORM-05: API Must Not Reference Runtime or Engines
**Source:** platform.guard.md (G-PLATFORM-05)

BLOCK if any file in `/src/platform/api/` references:
- `src/runtime` namespaces (`Whyce.Runtime.*`)
- `src/engines` namespaces (`Whyce.Engines.*`)
- `infrastructure` namespaces

#### R-PLAT-06 — G-PLATFORM-06: API Calls Systems Layer Only
**Source:** platform.guard.md (G-PLATFORM-06)

API controllers MUST dispatch through `ISystemIntentDispatcher` or shared contracts only. No direct engine or runtime type usage in controller code.

#### R-PLAT-07 — G-PLATFORM-07: Host Wires All Layers
**Source:** platform.guard.md (G-PLATFORM-07)

Host (`Program.cs`) is the composition root and MAY reference runtime, engines, systems, domain, and infrastructure for DI registration purposes only.

#### R-PLAT-08 — PLAT-NO-DOMAIN-01
**Source:** platform.guard.md (NEW RULES INTEGRATED — 2026-04-07)

`src/platform/host/Program.cs` MUST NOT reference any concrete domain type. Per-domain wiring MUST be encapsulated in a bootstrap module (`*Bootstrap.cs`) under `src/platform/host/composition/` or `src/systems/` and invoked from `Program.cs` by a single non-typed call.

#### R-PLAT-09 — PLAT-RESOLVER-01
**Source:** platform.guard.md (NEW RULES INTEGRATED — 2026-04-07)

Host adapters MUST NOT hold static dictionaries keyed by concrete domain types. Event-type -> CLR-type mappings live in a runtime-side `EventSchemaRegistry`.

#### R-PLAT-10 — PLAT-KAFKA-GENERIC-01
**Source:** platform.guard.md (NEW RULES INTEGRATED — 2026-04-07)

Kafka projection consumer workers in `src/platform/host/adapters/**` MUST be generic over (topic, handler-resolver, schema-registry, projection-table-resolver). Per-domain workers (e.g. `KafkaTodoProjectionConsumerWorker`) are FORBIDDEN.

#### R-PLAT-11 — PLAT-DET-01
**Source:** platform.guard.md (NEW RULES INTEGRATED — 2026-04-07)

Within `src/platform/host/adapters/**`, `Guid.NewGuid()` and `DateTime*.UtcNow` are FORBIDDEN. Use injected `IIdGenerator` (with deterministic seed derived from aggregate id+version) and `IClock`. Sole exception: `SystemClock` itself.

#### R-PLAT-12 — PLAT-DISPATCH-ONLY-01 (S1)
**Source:** platform.guard.md (NEW RULES INTEGRATED — 2026-04-08 Phase 1 gate blockers)
**Severity:** S1

All controller actions in `src/platform/api/controllers/**` MUST enter the runtime through `ISystemIntentDispatcher.DispatchAsync` and return `CommandResult` (with the full `auditEmission` envelope — `PolicyEvaluatedEvent`, `DecisionHash`, `ExecutionHash`, correlation/causation IDs). Direct `IIntentHandler.HandleAsync` calls from controllers are FORBIDDEN. Two parallel paths into the runtime = structural drift. DRIFT-4. Source: `claude/new-rules/_archives/20260408-000000-phase1-gate-blockers.md`.

#### Platform — Check Procedure
**Source:** platform.guard.md

1. Scan `/src/platform/api/` for `using` directives referencing `Whyce.Runtime.*`, `Whyce.Engines.*`, or `Whyce.Infrastructure.*`.
2. Scan `/src/platform/host/` for `[ApiController]` attribute declarations.
3. Scan `/src/platform/host/` for `MapGet`, `MapPost`, `MapPut`, `MapDelete` endpoint definitions.
4. Verify `/src/platform/api/*.csproj` references ONLY `Whycespace.Shared` (no Runtime, Engines, Domain, Systems, Projections).
5. Verify `/src/platform/host/*.csproj` references `Whycespace.Api` for controller discovery.
6. Verify all controllers reside in `/src/platform/api/controllers/`.
7. Verify namespace alignment: api files use `Whyce.Platform.Api.*`, host files use `Whyce.Platform.Host.*`.

#### Platform — Severity Levels
**Source:** platform.guard.md

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | API references engine/runtime directly | `using Whyce.Engines.T2E;` in controller |
| **S0 — CRITICAL** | Controller in host | `[ApiController]` in `/src/platform/host/` |
| **S1 — HIGH** | API registers infrastructure in DI | `services.AddSingleton<IEventStore>()` in api |
| **S1 — HIGH** | Host exposes HTTP endpoints | `app.MapGet("/api/...")` in Program.cs |
| **S2 — MEDIUM** | Namespace misalignment | Api file using `Whyce.Platform.Host.*` namespace |

#### Platform — Enforcement Action
**Source:** platform.guard.md

- **S0**: Block merge. Fail CI. Immediate remediation.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.

All violations produce a structured report:
```
PLATFORM_GUARD_VIOLATION:
  file: <path>
  rule: <G-PLATFORM-XX>
  severity: <S0-S3>
  violation: <description>
  expected: <correct placement>
  actual: <detected violation>
  remediation: <fix instruction>
```

---

### Section: Systems Integration

Source: `systems.guard.md`. `src/systems/` is a pure composition layer. Systems wire engines together via runtime, define workflow topologies, and declare system boundaries — but they must never execute domain logic, mutate domain state, or perform direct persistence.

#### R-SYS-01 — Composition Only
**Source:** systems.guard.md (Rule 1)

Systems files must contain only composition logic: wiring engines, declaring pipelines, configuring runtime behavior, and defining system boundaries. No execution of business operations. No domain method invocations. No data transformations beyond routing metadata.

#### R-SYS-02 — No Execution Logic
**Source:** systems.guard.md (Rule 2)

Systems must not contain loops that process data, conditionals that evaluate business rules, or algorithms that transform domain state. If a system file contains `for`, `while`, `foreach` over domain collections, or `if/else` on domain properties for business purposes, it is a violation.

#### R-SYS-03 — No Domain Mutation
**Source:** systems.guard.md (Rule 3)

Systems must never create, update, or delete domain aggregates. No calls to aggregate constructors, factory methods, `Apply()`, `Create()`, `Update()`, `Delete()`, or any method that modifies aggregate state. Domain mutation is exclusively the responsibility of T2E engines invoked through runtime.

#### R-SYS-04 — No Direct Persistence
**Source:** systems.guard.md (Rule 4)

Systems must not interact with databases, file systems, or external storage. No `DbContext`, `IRepository.Save()`, `File.Write()`, or storage SDK calls. Persistence is managed by infrastructure adapters invoked through the runtime pipeline.

#### R-SYS-05 — Upstream / Midstream / Downstream Placement
**Source:** systems.guard.md (Rule 5)

Systems are classified by their position in the flow:
- **Upstream**: Ingress systems that receive external input and dispatch commands to runtime. Examples: API composition, webhook receivers.
- **Midstream**: Orchestration systems that compose multiple engine workflows via runtime. Examples: saga compositions, multi-step workflow definitions.
- **Downstream**: Egress systems that compose output projections and external notifications. Examples: notification composers, report assembly.

Each system must be placed in the correct category folder.

#### R-SYS-06 — Systems Compose Via Runtime
**Source:** systems.guard.md (Rule 6)

All engine invocation from systems must go through runtime. Systems declare which engines to wire and how commands flow, but never call engine methods directly. Systems configure the runtime pipeline; runtime executes it.

#### R-SYS-07 — No Domain Type Construction
**Source:** systems.guard.md (Rule 7)

Systems must not instantiate domain value objects, entities, or aggregates. Systems may reference domain types in generic type parameters (e.g., `ICommandHandler<CreateOrderCommand>`) but must not `new` domain objects.

#### R-SYS-08 — System Boundary Declaration
**Source:** systems.guard.md (Rule 8)

Each system must explicitly declare its boundary: which BCs it touches, which engines it composes, and which runtime pipelines it configures. This declaration serves as the contract for architectural auditing.

#### R-SYS-09 — No Infrastructure Imports
**Source:** systems.guard.md (Rule 9)

Systems must not import infrastructure types directly (HTTP clients, database drivers, message brokers). Integration with external systems is done through T3I engines composed via runtime.

#### R-SYS-10 — Idempotent Composition
**Source:** systems.guard.md (Rule 10)

System composition must be idempotent: applying the same system configuration twice must produce the same runtime pipeline. No ordering dependencies between system registrations unless explicitly declared.

#### R-SYS-11 — Systems Must Not Hold State
**Source:** systems.guard.md (Rule 11)

System composition classes must be completely stateless. No mutable instance fields, no caching, no accumulated state across invocations. Systems declare wiring topology and pipeline configuration — they do not maintain any runtime state. All state is held by domain aggregates (write-side) or projections (read-side), never by systems.

#### R-SYS-12 — Systems Must Not Evaluate Policy
**Source:** systems.guard.md (Rule 12)

Systems must not evaluate, check, or enforce policies. Policy evaluation is a runtime middleware concern. Systems compose pipelines that include policy middleware, but systems themselves never inspect policy decisions, check actor permissions, or evaluate authorization rules. Policy logic in systems is a layer violation.

#### R-SYS-13 — Systems Must Not Emit Events
**Source:** systems.guard.md (Rule 13)

Systems must not publish, raise, or dispatch domain events. Event emission is the exclusive responsibility of domain aggregates (raising events) and runtime (dispatching events). Systems compose the pipeline through which events flow, but they are not event producers. Any `IEventBus.Publish()` or event raising call in systems is a critical violation.

#### R-SYS-14 — SYS-BOUND-01 (CLARIFIED 2026-04-07)
**Source:** systems.guard.md (NEW RULES INTEGRATED — 2026-04-07)

Systems layer MUST access runtime ONLY through shared-contract surfaces under `Whyce.Shared.Contracts.Runtime.*`. The permitted surfaces are:
- `IWorkflowDispatcher` — for starting workflows from systems orchestrators
- `IRuntimeControlPlane` — for entry points that already hold a fully-built `CommandContext`
- `ISystemIntentDispatcher` — for entry points that hold only a bare command plus `DomainRoute` (the canonical bridge that internally constructs `CommandContext` via runtime middleware)

All three of these interfaces live in `src/shared/contracts/runtime/` and are shared-contract surfaces by namespace, by file location, and by design intent. Direct references from `src/systems/**` to runtime **implementations** (concrete classes under `Whyce.Runtime.*`) remain FORBIDDEN.

**Rationale for the clarification:** the prior wording forbade `ISystemIntentDispatcher` on the basis that it was "internal runtime dispatcher". That classification was incorrect — the type is in the shared contracts namespace, and it exists precisely because systems layer entry points (e.g. `WorkflowDispatcher` implementing `IWorkflowDispatcher`) do not hold a `CommandContext` and cannot construct one without invoking the policy / identity middleware that runs inside the runtime control plane. Forbidding it produced a perpetually-tracked violation that could only be "remediated" by either (a) adding a `DomainRoute` overload to `IRuntimeControlPlane` (cascade through every implementer and caller) or (b) collapsing `ISystemIntentDispatcher` into the runtime control plane (architectural decision out of scope for hardening).

**Scope of the permission:**
- Permitted in `src/systems/midstream/wss/WorkflowDispatcher.cs` (the canonical example) and any other systems-layer entry-point class that bridges API/external triggers into the runtime command pipeline.
- NOT a license to spray dispatcher calls across systems composition modules. Composition logic (per `## Rules` 1, 13 above) must remain composition-only.
- The E-STEP-02 concern (T1M workflow steps using `ISystemIntentDispatcher` from inside `src/engines/T1M/steps/`) is **separate** and remains under `engines.guard.md` authority. This clarification does not relax E-STEP-02.

#### R-SYS-15 — SYS-NO-STEP-01
**Source:** systems.guard.md (NEW RULES INTEGRATED — 2026-04-07)

`IWorkflowStep` implementations are FORBIDDEN under `src/systems/**` (see E-STEP-01).

#### Systems — Check Procedure
**Source:** systems.guard.md

1. Scan `src/systems/` for domain aggregate/entity/value-object instantiation (`new Aggregate()`, `Aggregate.Create()`, etc.).
2. Scan for domain mutation method calls (`.Apply()`, `.Update()`, `.Delete()`, `.AddItem()`, etc. on domain types).
3. Scan for business-logic conditionals (`if (order.Status ==`, `switch (payment.State)`, etc.).
4. Scan for data-processing loops operating on domain collections.
5. Scan for persistence calls (`DbContext`, `SaveChanges`, `IRepository`, `File.`, `BlobClient`).
6. Scan for direct engine method invocations (non-runtime dispatched calls to engine types).
7. Verify all systems are placed in upstream/midstream/downstream folders.
8. Scan for infrastructure type imports (HTTP, DB, message broker types).
9. Verify each system file declares its BC and engine boundary.
10. Verify composition is idempotent (no mutable static registration, no order-dependent init).

#### Systems — Pass Criteria
**Source:** systems.guard.md

- All systems files contain only composition and wiring logic.
- Zero domain mutation calls found.
- Zero direct persistence calls found.
- Zero business-logic conditionals found.
- Zero direct engine invocations (all through runtime).
- All systems properly categorized as upstream/midstream/downstream.
- No infrastructure imports in systems layer.

#### Systems — Fail Criteria
**Source:** systems.guard.md

- System file mutates domain aggregate state.
- System file executes business logic (conditional on domain state).
- System file performs direct persistence.
- System file invokes engine method directly (bypassing runtime).
- System file instantiates domain types.
- System file imports infrastructure types.
- System not categorized in upstream/midstream/downstream.
- System composition is not idempotent.

#### Systems — Severity Levels
**Source:** systems.guard.md

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Domain mutation in systems | `vault.Debit(amount)` in system file |
| **S0 — CRITICAL** | Direct persistence in systems | `_dbContext.SaveChangesAsync()` in system |
| **S1 — HIGH** | Business logic in systems | `if (invoice.IsPastDue)` in system |
| **S1 — HIGH** | Direct engine invocation | `_t2Engine.Execute(command)` in system |
| **S1 — HIGH** | Domain type construction | `new Money(100, Currency.USD)` in system |
| **S2 — MEDIUM** | Infrastructure import | `using Microsoft.Azure.ServiceBus;` |
| **S2 — MEDIUM** | Missing placement category | System not in upstream/midstream/downstream |
| **S2 — MEDIUM** | Data processing loop | `foreach (var item in orders)` with transformation |
| **S3 — LOW** | Missing boundary declaration | System without BC/engine boundary docs |
| **S3 — LOW** | Non-idempotent registration | Order-dependent system composition |

#### Systems — Enforcement Action
**Source:** systems.guard.md

- **S0**: Block merge. Fail CI. Immediate remediation required.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
SYSTEMS_GUARD_VIOLATION:
  system: <system name>
  placement: <upstream|midstream|downstream>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <composition-only behavior>
  actual: <detected execution/mutation>
  remediation: <fix instruction>
```

---

### Section: Kafka Event Fabric

Source: `kafka.guard.md`. Kafka must be accessed exclusively through the runtime layer using the dual-topic pattern (commands + events), with strict topic naming conventions and mandatory outbox pattern for event publishing.

#### R-K-01 — Kafka Used Only Through Runtime
**Source:** kafka.guard.md (Rule 1)

All Kafka producer and consumer interactions must be encapsulated within `src/runtime/` or `infrastructure/` adapters invoked by runtime. No direct Kafka client usage in `src/domain/`, `src/engines/`, `src/systems/`, or `src/platform/`. Domain and engines must be Kafka-agnostic.

#### R-K-02 — Dual-Topic Pattern
**Source:** kafka.guard.md (Rule 2)

Every bounded context uses two Kafka topic types:
- **Command topic**: Carries command messages for the BC. Pattern: `{classification}.{context}.{domain}.commands`
- **Event topic**: Carries domain events from the BC. Pattern: `{classification}.{context}.{domain}.events`

No mixed-purpose topics. Commands and events must never share a topic.

#### R-K-03 — Topic Naming Convention
**Source:** kafka.guard.md (Rule 3)

All Kafka topics must follow the naming pattern: `{classification}.{context}.{domain}.{type}` where type is `commands`, `events`, `deadletter`, or `retry`. Examples: `economic.capital.vault.commands`, `governance.decision.voting.events`. No ad-hoc topic names. No camelCase or PascalCase — use lowercase with dots.

#### R-K-04 — No Direct Kafka in Domain
**Source:** kafka.guard.md (Rule 4)

Domain layer must have zero references to Kafka types. No `IProducer<>`, `IConsumer<>`, `KafkaConfig`, `ProducerMessage`, or any Kafka SDK type in `src/domain/`. Domain events are raised by aggregates; Kafka is an infrastructure concern.

#### R-K-05 — No Direct Kafka in Engines
**Source:** kafka.guard.md (Rule 5)

Engines must not reference Kafka SDK types. Engines receive commands through runtime (which may source them from Kafka) and return results to runtime (which may publish to Kafka). Engines are transport-agnostic.

#### R-K-06 — Outbox Pattern Mandatory
**Source:** kafka.guard.md (Rule 6)

All domain event publishing to Kafka must use the outbox pattern. Events are first persisted to an outbox table within the same transaction as the aggregate state change, then a background process (relay) publishes them to Kafka. No direct Kafka publish within the command handling transaction.

#### R-K-07 — Consumer Group Naming
**Source:** kafka.guard.md (Rule 7)

Kafka consumer groups must follow: `{system-name}.{classification}.{context}.{domain}.{purpose}`. Examples: `wbsm.economic.capital.vault.projection`, `wbsm.governance.decision.voting.saga`. Consumer group names must be globally unique and self-documenting.

#### R-K-08 — Dead Letter Topic Required
**Source:** kafka.guard.md (Rule 8)

Every command and event topic must have a corresponding dead-letter topic: `{classification}.{context}.{domain}.{type}.deadletter`. Failed messages are routed to dead letter after retry exhaustion. No silent message drops.

#### R-K-09 — Schema Governance
**Source:** kafka.guard.md (Rule 9)

All Kafka messages must have a registered schema (Avro, Protobuf, or JSON Schema). Schema registry must validate message compatibility. No untyped or ad-hoc JSON payloads on Kafka topics. Schema evolution must be backward-compatible.

#### R-K-10 — No Kafka Configuration in Domain or Engines
**Source:** kafka.guard.md (Rule 10)

Kafka connection strings, topic configurations, consumer group settings, and broker addresses must not appear in domain or engine configuration. Kafka config lives in `infrastructure/` or runtime configuration only.

#### R-K-11 — Partition Key Alignment
**Source:** kafka.guard.md (Rule 11)

Kafka partition keys must align with aggregate IDs to ensure ordered processing per aggregate. Commands for the same aggregate must land on the same partition. Partition key = aggregate ID (or a deterministic derivative).

#### R-K-12 — Exactly-Once Semantics
**Source:** kafka.guard.md (Rule 12)

Kafka consumers must implement idempotent processing. Combined with the outbox pattern on the producer side, the system achieves effectively exactly-once delivery. Consumers must track processed message offsets or use idempotency keys.

#### R-K-13 — Runtime Outbox Only
**Source:** kafka.guard.md (Rule 13)

All Kafka event publishing must flow through the runtime outbox pattern. No direct `producer.ProduceAsync()` or `producer.Send()` calls outside the outbox relay process. The outbox relay is the sole Kafka producer in the system. Any component publishing directly to Kafka bypasses transactional guarantees and is a critical violation.

#### R-K-14 — Event Schema Versioning Required
**Source:** kafka.guard.md (Rule 14)

All events published to Kafka must include a schema version identifier. Schema evolution must be backward-compatible (consumers on older schema versions must still function). Schema registry must enforce compatibility checks on publish. Breaking schema changes require a new topic version.

#### R-K-15 — Order Guarantee by Aggregate Id
**Source:** kafka.guard.md (Rule 15)

Events for the same aggregate must be delivered and processed in order. Kafka partition key must equal aggregate ID (or a deterministic derivative). Consumer processing must respect partition ordering. Out-of-order processing for the same aggregate is a critical violation that breaks event sourcing consistency.

#### R-K-16 — K-TOPIC-01 (Domain-aligned outbox topic routing)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-07)

Outbox publishers MUST route events to domain-aligned Kafka topics derived from event metadata: `whyce.{classification}.{context}.{domain}.events`. Hardcoded default topics (e.g. `whyce.events`) are FORBIDDEN.

#### R-K-17 — K-TOPIC-02 (Topic declarations in create-topics.sh)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-07)

Every bounded context that emits events MUST declare its topics in `infrastructure/event-fabric/kafka/create-topics.sh`. Missing topic declarations fail bootstrap verification.

#### R-K-18 — K-TOPIC-CANONICAL-01 (NAMING STANDARD)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-07 NORMALIZATION)

Kafka topics MUST follow canonical format:

```
whyce.{cluster}.{context}.{event}
```

ENFORCEMENT:
- 4–5 segments ONLY
- lowercase ONLY
- event MUST be past tense

#### R-K-19 — K-DETERMINISTIC-01 (PARTITION RESOLUTION)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-07 NORMALIZATION)

Kafka partition MUST be deterministic.

ENFORCEMENT:
- `PartitionResolver` must use deterministic hash (FNV-1a or equivalent)

#### R-K-20 — K-TOPIC-COVERAGE-01 (S0)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-08 Phase 1 gate blockers)
**Severity:** S0

Every event type ever written to `outbox.topic` MUST have a corresponding topic provisioned in `infrastructure/event-fabric/kafka/create-topics.sh`. A startup-time guard MUST diff the set of distinct `outbox.topic` values against the broker topic list and fail fast on mismatch. DRIFT-1.

#### R-K-21 — K-OUTBOX-ISOLATION-01 (S0)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-08 Phase 1 gate blockers)
**Severity:** S0

Outbox publishers MUST catch produce exceptions per row, mark the row `status='failed'` with an error message column (or move it to a dead-letter table), and continue. A single bad row MUST NOT halt the publish loop or crash the host. DRIFT-2.

#### R-K-22 — K-TOPIC-DOC-CONSISTENCY-01 (S3)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-08 Phase 1 gate blockers)
**Severity:** S3

Any topic name written in `claude/` docs / prompts MUST match an existing topic in `create-topics.sh`. DRIFT-6.

#### R-K-23 — K-DLQ-PUBLISH-01 (S2)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-08 DLQ publish)
**Severity:** S2

Sub-clause of rule 8 (Dead Letter Topic Required). When an outbox row exhausts its retry budget, the publisher MUST attempt to produce the message body to the canonical deadletter topic `{classification}.{context}.{domain}.deadletter` BEFORE updating the row to `status='deadletter'`. Headers MUST include original topic, `event-type`, `correlation-id`, and `dlq-reason` (last error). "Configured" in rule 8 means "actively published to," not merely "exists in create-topics.sh." Source: `claude/new-rules/_archives/20260408-010000-kafka-dlq-publish.md`.

#### R-K-24 — K-HEADER-CONTRACT-01 (S1)
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-08 Kafka header contract)
**Severity:** S1

Sub-clause of rule 6 (OUTBOX PATTERN MANDATORY). All Kafka messages produced by the WBSM event fabric MUST carry the following headers populated from the canonical `EventEnvelope` discrete columns (NOT derived from payload bodies): `event-id` (UUID, from `EventEnvelope.EventId`), `aggregate-id` (UUID, from `EventEnvelope.AggregateId`), `event-type` (string), `correlation-id` (UUID). Consumers MUST NOT silently skip messages with missing headers — missing headers indicate a producer-side contract violation and must be surfaced (deadletter or halt), never absorbed. Source: `claude/new-rules/_archives/20260408-020000-kafka-header-contract.md`.

Phase 1 gate source: `claude/new-rules/_archives/20260408-000000-phase1-gate-blockers.md`.

#### R-K-25 — K-DLQ-001 (S0, data-loss risk): DLQ route precedes source-offset commit
**Source:** kafka.guard.md (NEW RULES INTEGRATED — 2026-04-10 promoted from new-rules backlog)
**Severity:** S0 (data-loss risk)

On the consumer failure path, a poisoned message MUST be successfully produced to its DLQ topic (and its DLQ produce acknowledged) BEFORE the source partition offset is committed. Committing the source offset prior to a confirmed DLQ write is a guard violation: a process death between the two acts permanently loses the message. The consumer wrapper MUST express this as: `await ProduceToDlqAsync(msg); await CommitSourceOffsetAsync(msg);` — never the reverse, never in parallel without an awaited DLQ ack. Static check: grep consumer wrappers for `Commit*` calls and assert each is dominated by an awaited DLQ produce on the failure branch. Source: `_archives/20260408-142631-validation.md` Finding 2.

#### Kafka — Check Procedure
**Source:** kafka.guard.md

1. Scan `src/domain/` for Kafka-related imports: `Confluent.Kafka`, `KafkaNet`, `IProducer`, `IConsumer`, `KafkaConfig`.
2. Scan `src/engines/` for the same Kafka-related imports.
3. Scan `src/systems/` and `src/platform/` for direct Kafka producer/consumer instantiation.
4. Verify all Kafka producer/consumer code resides in `src/runtime/` or `infrastructure/`.
5. Enumerate all topic names and validate against the naming pattern `{classification}.{context}.{domain}.{type}`.
6. Verify dual-topic pattern: for each BC, confirm both `.commands` and `.events` topics exist.
7. Verify outbox table and relay exist: scan for outbox entity and background relay service.
8. Verify dead-letter topics exist for each command and event topic.
9. Verify consumer group names follow the naming convention.
10. Scan for direct `producer.ProduceAsync()` or `producer.Send()` within command handlers (must use outbox instead).
11. Verify partition key configuration uses aggregate ID.
12. Verify schema registry references for all topic configurations.

#### Kafka — Pass Criteria
**Source:** kafka.guard.md

- Zero Kafka imports in domain or engine layers.
- All topics follow naming convention.
- Dual-topic pattern applied for all active BCs.
- Outbox pattern in use for all event publishing.
- Dead-letter topics configured for all topics.
- Consumer groups follow naming convention.
- Schema governance in place.
- Partition keys aligned with aggregate IDs.

#### Kafka — Fail Criteria
**Source:** kafka.guard.md

- Kafka import in domain or engine file.
- Direct Kafka producer/consumer in platform or systems layer.
- Topic name violating naming convention.
- Missing dual-topic (commands or events topic absent for active BC).
- Event published directly to Kafka without outbox.
- Missing dead-letter topic.
- Consumer group with ad-hoc name.
- Untyped Kafka message without schema.
- Kafka configuration in domain or engine config.

#### Kafka — Severity Levels
**Source:** kafka.guard.md

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Kafka import in domain | `using Confluent.Kafka;` in aggregate file |
| **S0 — CRITICAL** | Direct publish without outbox | `producer.ProduceAsync(topic, msg)` in command handler |
| **S1 — HIGH** | Kafka import in engine | `IProducer<string, byte[]>` in T2E engine |
| **S1 — HIGH** | Topic naming violation | Topic named `VaultCommands` instead of `economic.capital.vault.commands` |
| **S1 — HIGH** | Missing dead-letter topic | No `.deadletter` topic for an active BC |
| **S2 — MEDIUM** | Direct Kafka in platform | Controller creating Kafka producer |
| **S2 — MEDIUM** | Missing dual-topic | BC has event topic but no command topic |
| **S2 — MEDIUM** | Ad-hoc consumer group name | Consumer group named `myConsumer1` |
| **S3 — LOW** | Missing schema registration | Topic without schema validation |
| **S3 — LOW** | Partition key not aggregate ID | Random or hash-based partition key |

#### Kafka — Enforcement Action
**Source:** kafka.guard.md

- **S0**: Block merge. Fail CI. Immediate remediation.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for tech debt.

All violations produce a structured report:
```
KAFKA_GUARD_VIOLATION:
  file: <path>
  topic: <topic name if applicable>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct Kafka usage>
  actual: <detected violation>
  remediation: <fix instruction>
```

---

### Section: Config & Secret Safety

Source: `config-safety.guard.md`.

**Status:** ACTIVE
**Severity baseline:** S0/S1 violations FAIL the build.
**Owner:** WBSM v3 structural integrity.

Scope for this section: all files under `src/`, `infrastructure/`. Test fixtures under `tests/` are explicitly out of scope unless they hit production-shared files.

#### R-CFG-R1 — No credentials in source-controlled config files (S0)
**Source:** config-safety.guard.md (CFG-R1)
**Severity:** S0

**Forbidden in `appsettings*.json`, `*.config`, `**/docker-compose*.yml`, `**/*.compose.yml`, `**/compose.yaml`, and any committed config file:**
- Username/Password key-value pairs
- API keys, secret keys, access tokens
- Any string matching `Password=`, `Pwd=`, `SecretKey`, `AccessKey`, `ApiKey`, `Token=`
- Any non-empty value under keys ending in `Password`, `SecretKey`, `AccessKey`
- In docker-compose/compose files: literal `Password=`, `SecretKey:`, `AccessKey:`, `APIKey:`, `Token:` with non-`${...}` right-hand-sides

**Required form for compose files:** `${VAR_NAME}` substitution, with the variable defined in a gitignored `.env.local`.

**Exception:** None. Use environment variables exclusively.

#### R-CFG-R2 — No hardcoded production endpoint defaults in source-controlled config (S1)
**Source:** config-safety.guard.md (CFG-R2)
**Severity:** S1

Forbidden literal values in `appsettings*.json` for keys representing endpoints (`*ConnectionString`, `*BootstrapServers`, `*Endpoint`, `*Url`, `*Host`, `*Port`):
- `localhost`, `127.0.0.1`, `0.0.0.0`
- Any port literal (`5432`, `6379`, `9092`, `29092`, `8181`, `9000`, etc.)
- Any URL literal

Endpoints MUST be sourced from environment variables at startup. Composition root MUST throw `InvalidOperationException` when missing — no `??` fallback.

#### R-CFG-R3 — No hardcoded fallbacks in C# composition code (S1)
**Source:** config-safety.guard.md (CFG-R3)
**Severity:** S1

Forbidden pattern in any `.cs` file under `src/`:
```csharp
configuration["X"] ?? "Host=...;Password=..."
configuration.GetValue<string>("X") ?? "literal-default"
```
Required pattern (no-fallback):
```csharp
var value = configuration.GetValue<string>("X")
    ?? throw new InvalidOperationException("X is required");
```

#### R-CFG-R4 — Use IOptions<T> over raw IConfiguration in business code (S2)
**Source:** config-safety.guard.md (CFG-R4)
**Severity:** S2

Controllers, handlers, services MUST inject strongly-typed `IOptions<TOptions>` (or `IOptionsMonitor<T>`). Direct `IConfiguration` indexer access is restricted to:
- `src/platform/host/composition/**` (composition root)
- `Program.cs`

Magic timeouts/intervals/retry counts in business logic must come from an Options class.

#### R-CFG-R5 — appsettings.json shape
**Source:** config-safety.guard.md (CFG-R5)

The committed `appsettings.json` MUST contain only:
- Logging configuration
- Feature flags (boolean)
- Domain-shaped constants (e.g., topic names that are part of the schema contract)
- Empty placeholder structure for env-bound keys (or omit them entirely)

#### R-CFG-DC1 — CFG-R1 Docker Compose Extension (S0)
**Source:** config-safety.guard.md (NEW RULES INTEGRATED — 2026-04-13)
**Severity:** S0

CFG-R1 file glob extended to include `**/docker-compose*.yml`, `**/*.compose.yml`, `**/compose.yaml`. Forbidden patterns: literal `Password=`, `SecretKey:`, `AccessKey:`, `APIKey:`, `Token:` with non-`${...}` right-hand-sides. Required form: `${VAR_NAME}` substitution with variable defined in gitignored `.env.local`. Source: `20260410-CFG-R1-DOCKER-COMPOSE-SCAN-guards.md`.

#### R-CFG-K1 — Configuration key form (S0)
**Source:** config-safety.guard.md (NEW RULES INTEGRATED — 2026-04-10)
**Severity:** S0

Configuration key lookups MUST use the `Section:Key` form, never `Section__Key`. The double-underscore form is the env-var encoding consumed by `AddEnvironmentVariables()`, which rewrites `Foo__Bar` to config key `Foo:Bar`. A literal `GetValue<string>("Foo__Bar")` lookup therefore never resolves env vars and silently throws (or silently uses a fallback) on every required key. CI grep: `GetValue<.*>\(\"[A-Za-z0-9_]+__` = S0 fail. Originating evidence: 15 broken callsites in `src/platform/host/composition/**` (`InfrastructureComposition.cs`, `ObservabilityComposition.cs`, `TodoBootstrap.cs`) blocking all live execution. Source: `_archives/20260408-145000-validation-live-execution.md` Finding 5.

#### Config — CI Enforcement
**Source:** config-safety.guard.md

1. **Architecture test:** scan `src/platform/host/appsettings*.json` for `Password=`, `SecretKey`, `AccessKey`, `localhost`, port literals — fail on any hit.
2. **Architecture test:** grep `src/**/*.cs` for `?? "Host=` and `?? "Server=` and `configuration\[` outside composition — fail on any hit.
3. **Build:** composition root must be wired to throw on missing env vars (verified by integration test that boots host with empty environment and asserts `InvalidOperationException`).
4. **Architecture test:** scan `**/docker-compose*.yml`, `**/*.compose.yml`, `**/compose.yaml` for literal `Password=`, `SecretKey:`, `AccessKey:`, `APIKey:`, `Token:` with non-`${...}` values — fail on any hit.

#### Config — Allowed Exceptions
**Source:** config-safety.guard.md

- `src/platform/host/composition/observability/ObservabilityComposition.cs` and `InfrastructureComposition.cs` may directly read `IConfiguration` to bootstrap options.
- Health-check timeouts may be hardcoded (not part of operational SLA).
- Domain-shaped Kafka topic constants (e.g., `whyce.operational.sandbox.todo.events`) are schema contracts, not configuration.

#### Config — Remediation Template
**Source:** config-safety.guard.md

```csharp
// BEFORE
var cs = configuration["EventStore__ConnectionString"]
    ?? "Host=localhost;Username=whyce;Password=whyce";

// AFTER
var cs = configuration.GetValue<string>("EventStore:ConnectionString")
    ?? throw new InvalidOperationException(
        "EVENTSTORE__CONNECTIONSTRING environment variable is required");
```

---

### Section: Composition Loader

Metadata: `name: composition-loader`, `type: structural`, `severity: S1`, `locked: true`.

**Source:** absorbed from `claude/guards/composition-loader.guard.md`.

**Purpose:** Lock the deterministic composition module loader. The loader must remain the sole entry point for category composition wiring inside `Program.cs`, and the `CompositionRegistry` must remain explicit, ordered, and reflection-free.

#### G-COMPLOAD-01 — Registry Membership

FAIL IF any class implementing `ICompositionModule` is not listed in `src/platform/host/composition/registry/CompositionRegistry.cs`.

#### G-COMPLOAD-02 — Explicit Order

FAIL IF any `ICompositionModule` implementation does not define a unique, non-negative integer `Order`. Duplicate or missing `Order` values are S1.

#### G-COMPLOAD-03 — Locked Execution Sequence

FAIL IF the registry order deviates from:
`Core(0) → Runtime(1) → Infrastructure(2) → Projections(3) → Observability(4)`.
Adding a new module requires extending this sequence and updating this guard.

#### G-COMPLOAD-04 — Loader-Only Composition

FAIL IF `Program.cs` re-introduces direct `Add*Composition(...)` calls instead of `builder.Services.LoadModules(builder.Configuration)`.

#### G-COMPLOAD-05 — BootstrapModuleCatalog Preserved

FAIL IF the `BootstrapModuleCatalog.All` registration loop is removed from `Program.cs` or migrated into the composition loader. Domain bootstrap MUST remain a separate, explicit pass.

#### G-COMPLOAD-06 — No Reflection Discovery

FAIL IF the loader, registry, or any composition module discovers types via reflection (`Assembly.GetTypes`, `Activator.CreateInstance`, attribute scans, etc.). Module enumeration is explicit list literals only.

#### G-COMPLOAD-07 — Modules Are Orchestration-Only

FAIL IF any `ICompositionModule.Register` body contains anything beyond a single delegating call to its category `Add*Composition` extension. No `new`, no `services.AddSingleton<...>` calls inside modules themselves.

**Severity:** All G-COMPLOAD-* rules: **S1 — HIGH** (block merge).

---

### Section: Program Composition

Metadata: `name: program-composition`, `type: structural`, `severity: S1`, `locked: true`.

**Source:** absorbed from `claude/guards/program-composition.guard.md`.

**Purpose:** Lock `src/platform/host/Program.cs` to composition-only orchestration. All service registration must live inside category composition modules under `src/platform/host/composition/{category}/`.

#### G-PROGCOMP-01 — Composition Only

FAIL IF `Program.cs` contains any of:
- `builder.Services.AddSingleton<` / `AddTransient<` / `AddScoped<`
- `builder.Services.AddHostedService(`
- Direct `new` of any infrastructure adapter or middleware
- `Configuration.GetValue<` / `Configuration["..."]` reads

ALLOWED:
- `WebApplication.CreateBuilder` and `builder.Build()`
- Calls to `Add*Composition(...)` extension methods
- Calls to `LoadModules(...)` from `CompositionModuleLoader` (deterministic registry walk)
- Calls to bootstrap module `RegisterServices` from `BootstrapModuleCatalog`
- HTTP pipeline configuration (`app.Use*`, `app.Map*`)
- `app.Run()`

#### G-PROGCOMP-02 — Size Cap

`Program.cs` MUST NOT exceed 100 non-empty lines. Re-extract before crossing this threshold.

#### G-PROGCOMP-03 — Classification-Aligned Domain Wiring

Domain registration MUST flow through `IDomainBootstrapModule` instances listed in `BootstrapModuleCatalog`. No domain type may be referenced directly from `Program.cs` or from any non-domain composition module.

#### G-PROGCOMP-04 — No Inline Middleware Definition

`Program.cs` MUST NOT define new middleware classes inline or via lambdas that contain business logic. Middleware composition belongs in `composition/runtime/RuntimeComposition.cs`.

#### G-PROGCOMP-05 — Locked Pipeline Order

The HTTP pipeline order in `Program.cs` MUST remain:
`HttpMetricsMiddleware → UseRouting → UseSwagger → UseSwaggerUI → MapControllers → MapMetrics → Run`. The locked runtime middleware order inside `RuntimeComposition` is enforced by `runtime.guard.md` §Execution Pipeline & Ordering.

**Severity:**
- G-PROGCOMP-01, G-PROGCOMP-03, G-PROGCOMP-04, G-PROGCOMP-05: **S1 — HIGH** (block merge)
- G-PROGCOMP-02: **S2 — MEDIUM** (must resolve in current sprint)

---

## WBSM v3 Global Enforcement (Cross-Cutting)

The following global enforcement block appears in both `systems.guard.md` and `kafka.guard.md`. It is consolidated here once and applies to all sections above.

**Sources:** systems.guard.md (GE-01..GE-05), kafka.guard.md (GE-01..GE-05) — identical text, deduplicated.

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

## Dedup Notes

The following exact overlaps were deduplicated during consolidation (semantics preserved):

1. **WBSM v3 GLOBAL ENFORCEMENT (GE-01 through GE-05)** appeared verbatim in both `systems.guard.md` and `kafka.guard.md`. Consolidated into a single cross-cutting section at the end. No semantic content removed.

No other exact duplicates were detected. All rule text from all four source files is preserved either verbatim or near-verbatim.

## Semantic Conflicts

None observed. Several rules reinforce one another across sections (e.g., `R-K-01` — no direct Kafka in systems/platform — and `R-SYS-09` — no infrastructure imports in systems — are mutually reinforcing, not conflicting). `R-PLAT-06` (API dispatches through `ISystemIntentDispatcher`) aligns with `R-SYS-14` (systems access runtime through shared-contract surfaces including `ISystemIntentDispatcher`); both describe the canonical entry-path and are complementary.
