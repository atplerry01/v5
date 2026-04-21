---
name: E1 → EX Unified Delivery Prompt (sections 1–18 + infrastructure contracts)
type: reusable-prompt-template
template-version: 2.0
status: locked
covers: All 18 sections of E1 → EX Delivery Pattern + section 06 infrastructure contracts
supersedes: domain-implementation-prompt.md (v1.0)
verified-by:
  - claude/audits/e1-ex-domain.audit.md
  - claude/guards/constitutional.guard.md
  - claude/guards/domain.guard.md
  - claude/guards/runtime.guard.md
  - claude/guards/infrastructure.guard.md
---

# E1 → EX Unified Delivery Prompt — {{VERTICAL}}

> **Single source of truth** for delivering a vertical bounded system from domain → engine → runtime → infrastructure → API → projections → tests → docs, aligned with the five canonical classifications: **structural-system, business-system, content-system, economic-system, operational-system**.
>
> **How to use this file**
>
> 1. Copy this file to `claude/project-prompts/{YYYYMMDD-HHMMSS}-{{VERTICAL}}-delivery.md` per CLAUDE.md $2.
> 2. Fill `{{VERTICAL}}`, `{{CONTEXTS}}`, `{{BC_LIST}}`, `{{TARGET_D_LEVEL}}`, and the scope-specific placeholders per phase.
> 3. Remove this "How to use" block after filling placeholders.
> 4. Execute **only the phases declared in scope**. Each phase has its own exit gate; do not advance until the prior phase's gate is green.
> 5. Audit sweep (CLAUDE.md $1b) runs automatically post-execution against every `claude/audits/*.audit.md`.

---

## TITLE

E1 → EX Delivery — {{VERTICAL}} vertical, phases {{PHASES_IN_SCOPE}}, target level {{TARGET_D_LEVEL}}.

## CONTEXT

The E1 → EX Delivery Pattern was extracted from the proven **economic-system** Phase 2 implementation (the E1 exemplar) and is the canonical skeleton for every subsequent vertical (E2 = structural, E3 = content, E4 = business, E5 = operational, and any additional vertical).

Exemplar artifact counts (economic, verified 2026-04-21):

| Dimension | Count | Path |
|---|---|---|
| Contexts (2nd-level) | 13 | `src/domain/economic-system/{capital,compliance,…,vault}/` |
| Domain BCs (3rd-level) | 42 | `src/domain/economic-system/{ctx}/{dom}/` |
| Aggregates | 40 | `src/domain/economic-system/**/aggregate/*Aggregate.cs` |
| Domain events | 140 | `src/domain/economic-system/**/event/*Event.cs` |
| T2E handlers | 128 | `src/engines/T2E/economic/**/*Handler.cs` |
| T1M workflows (state) | 12 | `src/engines/T1M/domains/economic/**/state/*WorkflowState.cs` |
| T1M steps | 33 | `src/engines/T1M/domains/economic/**/steps/*Step.cs` |
| Projection handlers | 41 | `src/projections/economic/**/*ProjectionHandler.cs` |
| Projection reducers | 40 | `src/projections/economic/**/reducer/*ProjectionReducer.cs` |
| Controllers | 39 | `src/platform/api/controllers/economic/**/*Controller.cs` |
| Policies (rego) | 31 | `infrastructure/policy/domain/economic/**/*.rego` |
| Kafka topic files | 12 | `infrastructure/event-fabric/kafka/topics/economic/**/topics.json` |
| Tests (unit / int / e2e) | 20 / 34 / 53 | `tests/{unit,integration,e2e}/economic-system|economic/` |

**Target vertical state** (fill per execution):

- Classification: `src/domain/{{VERTICAL}}-system/`
- Contexts: {{CONTEXTS}}
- Bounded contexts in scope: {{BC_LIST}}
- Current D-level per BC: {{CURRENT_D_LEVELS}}
- Target D-level: {{TARGET_D_LEVEL}} (D1 minimum; D2 requires all 18 phases)
- Exemplar to mirror: {{EXEMPLAR}} (default `src/domain/economic-system/capital/account/`)

---

## OBJECTIVE

Deliver {{BC_LIST}} up to {{TARGET_D_LEVEL}}:

- **D0 (Scaffold)** — folder structure with `.gitkeep` placeholders (phase 1 only).
- **D1 (Partial)** — phases 1–6 complete: domain layer shipped, zero S0/S1 violations.
- **D2 (Active)** — phases 1–18 complete: engine, runtime, infrastructure, API, projections, tests, docs all green. Consumable by production compositions per `domain.guard.md` rule 10.

The audit sweep produces:
- `claude/audits/e1-ex-domain.audit.md` — phases 1–6 gate.
- All four canonical guards re-pass per CLAUDE.md $1b.

---

## CONSTRAINTS

### Guards (per CLAUDE.md $1a — reload fresh every run)

1. **constitutional.guard.md** — determinism (no `Guid.NewGuid`, `DateTime.*`, `Random`), deterministic IDs (HSID G5/G6), INV-REPLAY-LOSSLESS-VALUEOBJECT-01, policy authority (POL-01/02/05).
2. **domain.guard.md** — layer purity ($7), classification/context/domain topology (DS-R1..R8), strongly-typed identifiers (D-VO-TYPING-01), no BCL exceptions (D-ERR-TYPING-01), lifecycle-init idempotency (DOM-LIFECYCLE-INIT-IDEMPOTENT-01), DTO naming (DTO-R1..R4), GUARD-LAYER-MODEL-01.
3. **runtime.guard.md** — 8-stage middleware order, HSID single-stamp-point, no-dead-code / stub-detection, code quality subsystem, test & E2E validation subsystem.
4. **infrastructure.guard.md** — platform/systems/kafka/config-safety, composition loader, topic naming.

### Anti-Drift (CLAUDE.md $5)

- No new architecture patterns. Mirror the exemplar exactly.
- No file moves outside `src/{domain,engines,projections,runtime,platform,shared}/` and the vertical's classification root.
- No renaming of shared-kernel types.
- No file deletions without explicit capture under `claude/new-rules/`.

### Layer Purity (CLAUDE.md $7)

- Domain — zero external dependencies. No DI, no ORM, no logging, no HTTP, no provider SDKs.
- Engines — no DB / Kafka / HTTP / Redis / OPA SDK calls. Only `ISystemIntentDispatcher`, `IAggregateRepository<T>`, and sibling shared-contract interfaces.
- Runtime — sole owner of persistence, publish, anchor, policy-evaluate, HSID-stamp.
- Projections — reducers are pure; handlers use `PostgresProjectionStore<T>` + `IEnvelopeProjectionHandler`.
- API — controllers map HTTP → command; never touch domain or engine directly.

### Policy ($8) — WHYCEPOLICY

- Every command has a registered policy ID (POL-01/PB-01). Default deny (POL-02).
- Policies live at `infrastructure/policy/domain/{{VERTICAL}}/{ctx}/{dom}.rego`.
- Evaluation happens only at runtime middleware stage 2 via `IPolicyEvaluator`.

### Determinism ($9)

- No `Guid.NewGuid()`, no `DateTime.Now/UtcNow`, no `new Random(...)` in domain or engines.
- IDs via `IIdGenerator` (HSID), stamped at the runtime control-plane prelude (G6 single-stamp-point) — domain and engines receive VO-wrapped IDs, never generate them.
- Clocks via `IClock` (runtime-injected); domain receives `Timestamp` VOs.

### Events ($10)

- All state changes emit past-tense events named `{Subject}{Verb}Event`.
- Event parameter carrying the aggregate identifier has `[property: JsonPropertyName("AggregateId")]` per INV-REPLAY-LOSSLESS-VALUEOBJECT-01.

---

## EXECUTION STEPS (18 phases)

Each phase has: **scope**, **deliverables**, **verification**, **exit gate**. Run in order. Skip a phase only if it is explicitly out-of-scope for this execution.

### Phase 1 — System Definition & Mapping

- **Scope:** canonical scope, boundaries, terminology per vertical.
- **Deliverables:**
  - `src/domain/{{VERTICAL}}-system/README.md` listing every context and BC with its D-level.
  - Per-BC `README.md` at `src/domain/{{VERTICAL}}-system/{ctx}/{dom}/README.md` (≤15-term glossary, scope, naming rules).
- **Verification:** `find src/domain/{{VERTICAL}}-system -name README.md` returns one per BC plus vertical root.
- **Exit gate:** all READMEs exist.

### Phase 2 — Core Model

- **Scope:** aggregates, entities, value objects, state model.
- **Deliverables per BC:**
  - **MUST** folders: `aggregate/`, `error/`, `event/`, `value-object/`.
  - **WHEN-NEEDED** folders: `entity/` (when child entities exist), `service/` (when stateless coordination is needed), `specification/` (when invariants need composable predicates). Omit if the domain genuinely has none.
  - At least one aggregate class per BC.
  - Identifier VO (`{Concept}Id`) and lifecycle-state VO (`{Concept}Status`) where applicable.
- **Verification:** for each BC, `ls src/domain/{{VERTICAL}}-system/{ctx}/{dom}/` shows the MUST folders; WHEN-NEEDED folders present **iff** the domain requires them (document the decision in the BC's README).
- **Exit gate:** all MUST folders present; WHEN-NEEDED decisions recorded.

### Phase 3 — Identity & Reference

- **Scope:** strongly-typed IDs per D-VO-TYPING-01.
- **Deliverables:** every identifier VO is `readonly record struct {Concept}Id(Guid Value)` (or `Code` for string codes) with explicit constructor + `Guard.Against(...)` validation. Exemplar: [src/domain/economic-system/capital/account/value-object/AccountId.cs](../../../../src/domain/economic-system/capital/account/value-object/AccountId.cs).
- **Verification:** `grep -rEn 'public Guid \w+Id|Guid \w+Id[,)]|string \w+Id[,)]' src/domain/{{VERTICAL}}-system` returns empty.
- **Exit gate:** no raw `Guid` / `string` ID parameters anywhere in the vertical.

### Phase 4 — Domain Implementation

- **Scope:** concrete aggregates, events, specs, errors, services.
- **Deliverables per BC:**
  - Aggregate inherits `AggregateRoot`; private parameterless constructor + static factory (`Create`/`Open`/`Define`/…).
  - Lifecycle-init guard on first-event factories: `if (Version >= 0) throw <Aggregate>Errors.AlreadyInitialized();` per DOM-LIFECYCLE-INIT-IDEMPOTENT-01.
  - Every state mutation calls `RaiseDomainEvent(new {Subject}{Verb}Event(...))`.
  - Apply methods are `private void Apply({Event} evt)`; pattern-matched in base class.
  - Errors are static factories returning `DomainException` / `DomainInvariantViolationException` (zero raw BCL exception throws per D-ERR-TYPING-01).
- **Verification:**
  - `grep -rn 'Guid\.NewGuid\|DateTime\.\(Now\|UtcNow\)\|new Random' src/domain/{{VERTICAL}}-system` returns empty.
  - `grep -rn 'Microsoft\.Extensions\.DependencyInjection' src/domain/{{VERTICAL}}-system` returns empty.
  - `grep -rEn 'throw new (ArgumentException|ArgumentNullException|InvalidOperationException|NotImplementedException)' src/domain/{{VERTICAL}}-system` returns empty.
- **Exit gate:** [claude/audits/e1-ex-domain.audit.md](../../../audits/e1-ex-domain.audit.md) reports zero S0/S1.

### Phase 5 — Aggregate & Domain Design

- **Scope:** commands, events, queries per BC.
- **Deliverables per BC:**
  - Commands consolidated in `src/shared/contracts/{{VERTICAL}}/{ctx}/{dom}/{Domain}Commands.cs` (pattern per economic exemplar).
  - Events: `public sealed record {Subject}{Verb}Event([property: JsonPropertyName("AggregateId")] {Subject}Id {Subject}Id, …) : DomainEvent;`
  - ReadModel contracts colocated at `src/shared/contracts/{{VERTICAL}}/{ctx}/{dom}/{Domain}ReadModel.cs`.
  - EventSchema contracts at `src/shared/contracts/events/{{VERTICAL}}/{ctx}/{dom}/{Event}Schema.cs` (the projection-consumption surface).
- **Verification:** each BC has Commands + Events + ReadModel + EventSchema files.
- **Exit gate:** shared-contracts compiles; no orphan events/commands.

### Phase 6 — Business Invariants

- **Scope:** uniqueness, integrity, state-transition rules.
- **Deliverables:** pure `{Rule}Specification.cs` predicates under `specification/` — no I/O, no async, no `DateTime`, no DI. Aggregate uses `Guard.Against(...)` + specifications (no inline `if/throw`).
- **Exit gate:** domain-audit + `claude/audits/e1-ex-domain.audit.md` both green → **BC reaches D1**.

---

### Phase 7 — Policy Integration

- **Scope:** authorization + governance per command.
- **Deliverables:**
  - Rego policy per BC at `infrastructure/policy/domain/{{VERTICAL}}/{ctx}/{dom}.rego`.
  - Policy ID registered in policy registry (POL-05).
  - Every command references an explicit policy ID (POL-01/PB-01).
- **Verification:** `ls infrastructure/policy/domain/{{VERTICAL}}/` ≥ 1 `.rego` per BC with commands.
- **Exit gate:** middleware dispatches every command through policy eval; default deny test passes.

### Phase 8 — Runtime & Middleware Integration

- **Scope:** 8-stage pipeline wiring.
- **Deliverables:**
  - Command-dispatch routing in [src/runtime/dispatcher/RuntimeCommandDispatcher.cs](../../../../src/runtime/dispatcher/RuntimeCommandDispatcher.cs) includes every command and every aggregate-ID candidate VO.
  - Domain schema registration in [src/runtime/event-fabric/DomainSchemaCatalog.cs](../../../../src/runtime/event-fabric/DomainSchemaCatalog.cs) via `Register{{VERTICAL}}()`.
  - Per-type JSON converters in [src/runtime/event-fabric/EventDeserializer.cs](../../../../src/runtime/event-fabric/EventDeserializer.cs) for any multi-property VO used in event payloads.
  - `{{VERTICAL}}CompositionRoot.cs` under `src/platform/host/composition/{{VERTICAL}}/` added to `BootstrapModuleCatalog.All`.
- **Verification:** `grep -n '{{VERTICAL}}CompositionRoot' src/platform/host/composition/BootstrapModuleCatalog.cs` hits.

### Phase 9 — Application & Engine Services

- **Scope:** T1M + T2E engines.
- **Deliverables:**
  - **T2E** — one handler per single-shot command at `src/engines/T2E/{{VERTICAL}}/{ctx}/{dom}/{Action}{Aggregate}Handler.cs`. Handler loads aggregate via `IAggregateRepository<T>`, calls behavior, saves. Returns `RuntimeResult<T>` — never throws across boundary.
  - **T1M** — only when multi-step orchestration with compensation is required. Layout: `src/engines/T1M/domains/{{VERTICAL}}/{ctx}/state/{Workflow}WorkflowState.cs` + `src/engines/T1M/domains/{{VERTICAL}}/{ctx}/steps/{Action}Step.cs`. T1M is **not** required per context.
- **Engine forbidden list:** no `DbContext`, `IProducer<>`, `HttpClient`, `IConnectionMultiplexer`, engine-to-engine imports, or policy evaluations.
- **Verification:**
  - `grep -rEn 'DbContext|NpgsqlConnection|IProducer<|HttpClient' src/engines/T2E/{{VERTICAL}} src/engines/T1M/domains/{{VERTICAL}}` returns empty.
  - `grep -rn 'NotImplementedException\|// TODO\|// FIXME' src/engines/T2E/{{VERTICAL}} src/engines/T1M/domains/{{VERTICAL}}` returns empty.

### Phase 10 — Persistence & Event Sourcing

- **Scope:** event store, replay, VO JSON handling.
- **Deliverables:**
  - Every aggregate with wrapper-struct VOs in event payloads has a replay regression test per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`. Canonical template: [tests/integration/economic-system/vault/account/VaultAccountReplayRegressionTest.cs](../../../../tests/integration/economic-system/vault/account/VaultAccountReplayRegressionTest.cs).
  - Single-primitive wrapper VOs (`record struct X(Guid Value)`) auto-handled by `WrappedPrimitiveValueObjectConverterFactory` — no per-type converter needed.
  - Multi-property VOs (e.g., `Money(Amount, Currency)`) have per-type converters registered in `StoredOptions.Converters`.

### Phase 11 — Messaging & Event Fabric

- **Scope:** Kafka topics, outbox, retry, DLQ.
- **Deliverables:**
  - One `topics.json` per BC at `infrastructure/event-fabric/kafka/topics/{{VERTICAL}}/{ctx}/{dom}/topics.json`.
  - Declares **four** topics per BC following the canonical name pattern:
    ```
    whyce.{{VERTICAL}}.{ctx}.{dom}.commands
    whyce.{{VERTICAL}}.{ctx}.{dom}.events
    whyce.{{VERTICAL}}.{ctx}.{dom}.retry
    whyce.{{VERTICAL}}.{ctx}.{dom}.deadletter
    ```
  - Reference: `infrastructure/event-fabric/kafka/topics/economic/transaction/transaction/topics.json`.
  - Outbox drains through [src/platform/host/adapters/KafkaOutboxPublisher.cs](../../../../src/platform/host/adapters/KafkaOutboxPublisher.cs).

### Phase 12 — Projections & Read Models

- **Scope:** eventually-consistent read models per event.
- **Deliverables per BC:**
  - Handler at `src/projections/{{VERTICAL}}/{ctx}/{dom}/{Domain}ProjectionHandler.cs` implementing `IEnvelopeProjectionHandler` + one `IProjectionHandler<{Event}Schema>` per event type. Switch on `envelope.Payload`. Declare `ProjectionExecutionPolicy.Inline`.
  - Reducer at `src/projections/{{VERTICAL}}/{ctx}/{dom}/reducer/{Domain}ProjectionReducer.cs` — pure `(state, eventSchema) → state`, no I/O, no async.
  - ReadModel is **not** in the projections folder — it is a shared contract at `src/shared/contracts/{{VERTICAL}}/{ctx}/{dom}/{Domain}ReadModel.cs`.
  - Store: `PostgresProjectionStore<{Domain}ReadModel>` (concrete, injected via composition).
- **Verification:** every event type reaches at least one handler `switch` arm; reducer contains zero `async`, zero `HttpClient`, zero `DbContext`.

### Phase 13 — API & Platform Exposure

- **Scope:** controllers, DTOs, routes.
- **Deliverables:**
  - Controller at `src/platform/api/controllers/{{VERTICAL}}/{ctx}/{dom}/{Domain}Controller.cs`.
  - Dispatches commands via `ISystemIntentDispatcher` — no direct engine/domain access.
  - DTO naming per `domain.guard.md` DTO-R1..R4:
    - Request: `{Action}{Domain}RequestModel`
    - Response: `{Action}{Domain}ResponseModel` (or `Get{Domain}ResponseModel`)
    - Forbidden suffixes: `*Dto`, `*Data`, `*Info`, bare `*Response`/`*Request`.
  - Route `/api/{{VERTICAL}}/{ctx}/{dom}/{action}`.
- **Verification:** `grep -rEn 'return Ok\(\);$' src/platform/api/controllers/{{VERTICAL}}/` returns empty (no placeholder returns).

### Phase 14 — Observability & Evidence

- **Scope:** OTel metrics, tracing, WhyceChain anchoring.
- **Deliverables:**
  - Every T1M step increments `stepsExecuted`, `stepsFailed`, latency histogram via injected `Meter`.
  - Every T2E handler emits span per execution.
  - Event-store append triggers WhyceChain anchor via [src/runtime/](../../../../src/runtime/) control plane.

### Phase 15 — Cross-System Integration

- **Scope:** shared event contracts across verticals.
- **Deliverables:**
  - Cross-vertical events declared under `src/shared/contracts/events/{{VERTICAL}}/...` and consumed via schema subscription.
  - Document in vertical README which events cross which system boundaries.
  - Cross-system invariants (structural ↔ economic consistency, business ↔ economic attribution, content ↔ owning-system validation, operational observation-only) must be checked **before** event emission.

### Phase 16 — Testing & Certification

- **Scope:** three-tier test pyramid.
- **Deliverables:**
  - **Unit** — `tests/unit/{{VERTICAL}}-system/` — one file per aggregate / spec / multi-property VO.
  - **Integration** — `tests/integration/{{VERTICAL}}-system/` — one file per command (middleware → engine → event → projection in-memory).
  - **E2E** — `tests/e2e/{{VERTICAL}}/{ctx}/{dom}/` — full HTTP flow per BC, real Postgres + Kafka via test containers.
  - Replay regression test for every aggregate whose events carry wrapper-struct VOs.
- **Quality bar (floor, from economic exemplar):** unit ≥ 20, integration ≥ command-count × 0.5, e2e ≥ BC-count × 1.0.
- **Verification:** all three test tiers green.

### Phase 17 — Resilience & Recovery

- **Scope:** interruption, retry, projection recovery.
- **Deliverables:**
  - Projection rebuild from scratch produces identical state (drop + replay test).
  - Outbox retry topic exercised; deadletter inspection path documented.
  - Idempotency keys deterministic; replays are no-ops.

### Phase 18 — Documentation & Anti-Drift

- **Scope:** READMEs, audit updates, D2 promotion evidence.
- **Deliverables:**
  - Vertical README updated with final D-level per BC.
  - Per-BC README updated with event list, command list, policy ID list.
  - `claude/audits/{{VERTICAL}}.audit.md` (new) captures vertical-specific invariants.
  - Promotion record at `claude/new-rules/{YYYYMMDD-HHMMSS}-{{VERTICAL}}-d2-promotion.md` per CLAUDE.md $1c.
- **Exit gate:** all four canonical guards (`constitutional`, `runtime`, `domain`, `infrastructure`) re-pass. **BC reaches D2.**

---

## INFRASTRUCTURE CONTRACTS (section 06 inline reference)

The vertical's infrastructure boundary is expressed through real contracts — domain and engines consume these interfaces, never concrete providers.

| Concern | Canonical interface | File | Concrete adapter |
|---|---|---|---|
| Event persistence | `IEventStore` | `src/shared/contracts/infrastructure/persistence/IEventStore.cs` | Postgres event-store adapter |
| Outbox publish | `IOutbox` | `src/shared/contracts/infrastructure/messaging/IOutbox.cs` | `KafkaOutboxPublisher` |
| Event dispatch (in-process) | `IEventFabric` | `src/runtime/event-fabric/IEventFabric.cs` | `EventDispatcher` |
| Deadletter observation | `IDeadLetterStore` | `src/shared/contracts/infrastructure/messaging/IDeadLetterStore.cs` | Postgres-backed DLQ |
| Idempotency claim | `IIdempotencyStore` | `src/shared/contracts/infrastructure/persistence/IIdempotencyStore.cs` | Postgres |
| Sequence / HSID | `ISequenceStore` | `src/shared/contracts/infrastructure/persistence/ISequenceStore.cs` | Postgres |
| Distributed lease | `IDistributedLeaseProvider` | `src/shared/contracts/infrastructure/persistence/IDistributedLeaseProvider.cs` | Postgres advisory locks |
| Projection persistence | `PostgresProjectionStore<T>` | `src/shared/contracts/infrastructure/projection/` (+ handler interfaces) | Postgres |
| Cache | `IRedisClient` | `src/shared/contracts/infrastructure/projection/IRedisClient.cs` | Redis |
| Policy eval | `IPolicyEvaluator` | `src/shared/contracts/infrastructure/policy/IPolicyEvaluator.cs` | `OpaPolicyEvaluator` |

Delivery guarantees (enforced by these contracts):

- At-least-once delivery; possible duplication; no cross-aggregate ordering.
- Handlers MUST be idempotent; deduplication on `(eventId, handlerId)`.
- Exponential backoff + jitter with bounded attempts; overflow → `.deadletter` topic per BC.
- Replay rebuilds projections exactly per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`.
- Cache is disposable; never a source of truth; invalidation is explicit (TTL or event-driven).
- DocumentRef resolution happens in adapters; domain never fetches content.
- OPA owns authorization + governance; domain owns invariants; OPA does not override truth.

---

## OUTPUT FORMAT

Report at end of execution:

1. **Phases executed** — list with status (green / blocked / skipped-out-of-scope).
2. **Files created** — grouped by layer (domain, engines, runtime, projections, API, infra, tests, docs).
3. **Files modified** — grouped by layer, one-line change summary each.
4. **Pre-flight refactors** — aggregates lifted to canonical `AggregateRoot`, before/after.
5. **Quality-gate results** — every audit file pass/fail with evidence (grep hits, file counts).
6. **D-level progression** — per BC: pre → post.
7. **New-rules captured** — list under `claude/new-rules/` with one-line summary.
8. **Remaining gaps** — phases not in scope or blocked.

## VALIDATION CRITERIA

1. All four canonical guards pass per CLAUDE.md $1b.
2. `claude/audits/e1-ex-domain.audit.md` reports zero S0/S1 across the vertical (D1 minimum).
3. `dotnet build src/` exits 0 with zero warnings / zero errors.
4. No file modified outside `src/{domain,engines,projections,runtime,platform,shared}/{{VERTICAL}}*/**`, `infrastructure/{policy,event-fabric}/**/{{VERTICAL}}/**`, or `tests/**/{{VERTICAL}}*/**`.
5. Zero new `NotImplementedException`, zero new `// TODO` / `// FIXME` / `// HACK`.
6. Every command has a registered policy ID; default-deny e2e passes.
7. Replay rebuild produces identical projection state.
8. Every deliverable in the phases-in-scope has a grep-verifiable or file-existence-verifiable exit-gate signal.

## CLASSIFICATION

- Classification: `{{VERTICAL}}` (raw, no `-system` suffix per DS-R8 — used for `DomainRoute` / `CommandContext`).
- Context: Phase 2.X vertical delivery per `claude/project-topics/v2b/phase-2b.md`.
- Domain: `{{VERTICAL}}-system` full-stack delivery per E1 → EX template v2.0.

## SCOPE EXPLICITLY OUT

- Cross-vertical events shipping across more than the declared target boundary — add to a follow-up prompt.
- Deployment topology (helm, argo, k8s manifests) — infrastructure layer here is the in-process contract boundary, not the deployment artifact.
- Performance tuning, load testing, chaos injection — surface as follow-on work once D2 is held.
- Code-generation / scaffolding scripts — intentionally excluded per template v1.0 scope (see README).
