---
name: Domain Implementation Prompt (E1 → EX sections 1–6)
type: reusable-prompt-template
template-version: 1.0
status: locked
covers: E1-EX template sections 1–6 (domain layer)
verified-by: claude/audits/e1-ex-domain.audit.md
---

# Domain Implementation Prompt — {{VERTICAL}}

> **How to use this file**
>
> 1. Copy this file into `claude/project-prompts/{YYYYMMDD-HHMMSS}-{{VERTICAL}}-domain.md` per CLAUDE.md $2.
> 2. Fill the `{{VERTICAL}}`, `{{CONTEXTS}}`, `{{BC_LIST}}`, and `{{EXEMPLAR}}` placeholders.
> 3. Remove this "How to use" block after filling placeholders.
> 4. Execute the prompt. The post-execution sweep will run [e1-ex-domain.audit.md](../../../audits/e1-ex-domain.audit.md) automatically per CLAUDE.md $1b.

---

## TITLE

Domain Implementation — {{VERTICAL}} vertical, sections 1–6 per E1 → EX Delivery Pattern v1.0

## CONTEXT

The E1 → EX Delivery Pattern v1.0 ([claude/templates/e1-ex-delivery-pattern/](../README.md)) was extracted from the proven economic-system Phase 2 implementation. It defines the 18-section skeleton every vertical bounded system follows.

This prompt drives the **domain layer** portion (sections 1–6) of the **{{VERTICAL}}** vertical to completion:

- Section 1 — System Definition & Mapping
- Section 2 — Core Model
- Section 3 — Identity & Reference
- Section 4 — Domain Implementation
- Section 5 — Aggregate & Domain Design
- Section 6 — Business Invariants

Subsequent prompts in this vertical cover engines (7–9), runtime (10–11), projections (12), API (13), observability (14), cross-system (15), tests (16–17), and docs (18).

**Current state of the vertical** (to be filled per session):

- Classification folder: `src/domain/{{VERTICAL}}-system/`
- Contexts: {{CONTEXTS}}
- Bounded contexts to complete: {{BC_LIST}}
- Non-conformance notes: {{PRE_FLIGHT_FINDINGS}}

**Exemplar to mirror**: {{EXEMPLAR}} (default: [src/domain/economic-system/capital/account/](../../../../src/domain/economic-system/capital/account/))

## OBJECTIVE

Promote every BC in {{BC_LIST}} from its current D-level to **D1 (Partial)** at minimum — all sections 1–6 implemented — such that [e1-ex-domain.audit.md](../../../audits/e1-ex-domain.audit.md) passes with zero S0 / S1 violations for the vertical's classification path.

D2 (Active) requires sections 7–18 in subsequent prompts and is NOT in scope here.

## CONSTRAINTS

### Guards (per CLAUDE.md $1a — reload fresh)

- **constitutional.guard.md** — determinism (no `Guid.NewGuid`, `DateTime.*`, `Random`), deterministic IDs (`IIdGenerator`), hash determinism, INV-REPLAY-LOSSLESS-VALUEOBJECT-01.
- **domain.guard.md** — layer purity, DS-R1 / DS-R2 classification suffixes, D-VO-TYPING-01 (strongly-typed identifiers), D-ERR-TYPING-01 (no BCL exceptions), DOM-LIFECYCLE-INIT-IDEMPOTENT-01 (lifecycle-init guard), rules 1–24 core purity.
- **runtime.guard.md** — no-dead-code, stub-detection (zero `NotImplementedException`, zero `// TODO`, zero empty methods).
- **infrastructure.guard.md** — not directly in scope for domain layer.

### Anti-Drift (per CLAUDE.md $5)

- No new architecture patterns. Follow the exemplar exactly — aggregate structure, event pattern, VO shape, error factory, specification form.
- No renaming of existing types in {{VERTICAL}} unless required for guard conformance (e.g., bare `Cluster` → `ClusterAggregate`).
- No file moves outside `src/domain/{{VERTICAL}}-system/**`.
- Do not introduce new shared-kernel types. Consume existing `Whycespace.Domain.SharedKernel.*`.

### Layer Purity (per CLAUDE.md $7)

- Zero external dependencies. No `Microsoft.Extensions.DependencyInjection.*`, no ORM attributes, no HTTP types, no logging.
- Zero cross-BC references. Only shared-kernel VOs may be consumed from outside the BC.
- Zero policy logic in domain (that's a runtime concern).
- Zero direct database or infrastructure access.

### Identity (per CLAUDE.md $9)

- No `Guid.NewGuid()`. Deterministic IDs only via `IIdGenerator` — but the domain layer does NOT call `IIdGenerator` directly (HSID G5). IDs are provided by the calling engine/runtime and wrapped in VOs on the first line of entry-point factories.
- No system clock reads. `IClock` injection is a runtime concern; domain receives `Timestamp` VOs as method arguments.

## EXECUTION STEPS

### Step 0 — Pre-flight sweep

For each BC in {{BC_LIST}}:

1. Does the aggregate inherit `AggregateRoot` from shared-kernel? If not, refactor before anything else.
2. Does the aggregate track `Version` via the base class (not a manual field)?
3. Does the aggregate have `LoadFromHistory(IEnumerable<DomainEvent>)` support from the base class?
4. Does the aggregate use `_uncommittedEvents` manually, or `RaiseDomainEvent(...)` from the base?

Refactor any non-conformant aggregate to the canonical exemplar shape BEFORE adding new behavior. Pre-flight completion is the prerequisite for sections 2–6.

### Step 1 — System Definition & Mapping (section 1)

Deliverables per BC:

- A BC-level `README.md` at `src/domain/{{VERTICAL}}-system/{context}/{domain}/README.md` with: scope, boundaries, terminology, ubiquitous language glossary (≤15 terms), and naming conventions for events / commands / VOs specific to this BC.
- A vertical-level `README.md` at `src/domain/{{VERTICAL}}-system/README.md` listing every BC and its current D-level (D0/D1/D2).

Verification: `find src/domain/{{VERTICAL}}-system -name README.md` returns one per BC plus one at the vertical root.

### Step 2 — Core Model (section 2)

Deliverables per BC:

- All 7 mandatory artifact subfolders exist: `aggregate/`, `entity/`, `error/`, `event/`, `service/`, `specification/`, `value-object/`. Placeholder `.gitkeep` acceptable only for subfolders genuinely empty at D1.
- At least one aggregate class following the exemplar pattern.
- At least one value object for the aggregate's identity (e.g., `{Concept}Id`).
- At least one state / status value object if the aggregate has lifecycle states.

Verification: per [01-domain-skeleton.md](../01-domain-skeleton.md) "Folder layout" and "Mandatory artifact subfolders".

### Step 3 — Identity & Reference (section 3)

Deliverables per BC:

- All identifier VOs use the canonical `readonly record struct` form with explicit constructor + `Guard.Against(...)` validation. Reference: [src/domain/economic-system/capital/account/value-object/AccountId.cs](../../../../src/domain/economic-system/capital/account/value-object/AccountId.cs).
- Aggregate properties, event members, factory parameters, service methods, and specification constructors that hold an identifier MUST use the VO type, not raw `Guid` or `string` — per D-VO-TYPING-01.
- Single-primitive wrapper VOs keep canonical property names: `Value` (for primitives like Guid/int/decimal/DateTimeOffset) or `Code` (for string codes like Currency).

Verification: `grep -rEn 'public Guid \w+Id\|Guid \w+Id[,)]\|string \w+Id[,)]' src/domain/{{VERTICAL}}-system` returns empty (or every hit has a documented justification comment).

### Step 4 — Domain Implementation (section 4)

Deliverables per BC:

- No stub files (< 30 LOC aggregates with no behavior are a red flag).
- Every command path on the aggregate emits at least one past-tense domain event. No silent state mutation.
- Aggregate inherits `AggregateRoot` and uses `RaiseDomainEvent(...)` from the base class. No manual `_uncommittedEvents` list.
- Aggregate has a private parameterless constructor (for replay) + static factory method(s) (`Create`, `Define`, `Open`, etc.) for creation.
- Every Apply method handles exactly the events the aggregate raises.

### Step 5 — Aggregate & Domain Design (section 5)

Deliverables per BC:

- Every event class: `public sealed record {Subject}{PastTenseVerb}Event(...) : DomainEvent`.
- Every event that carries the aggregate identifier declares it with `[property: JsonPropertyName("AggregateId")]` on the parameter, per INV-REPLAY-LOSSLESS-VALUEOBJECT-01.
- Every aggregate has a lifecycle-init guard: `if (Version >= 0) throw <Aggregate>Errors.AlreadyInitialized();` on first-event methods, per DOM-LIFECYCLE-INIT-IDEMPOTENT-01.
- Every error is a static factory in `{Concept}Errors.cs` returning `DomainException` / `DomainInvariantViolationException`. Zero raw BCL throws per D-ERR-TYPING-01.
- Commands, if modeled in the domain, live alongside events (commands are typically defined in `src/shared/contracts/commands/` in this codebase — verify per-vertical convention).

### Step 6 — Business Invariants (section 6)

Deliverables per BC:

- Every transition rule (allowed state changes, uniqueness, parent-existence, child-capacity, cycle prevention) is expressed as a `{Rule}Specification.cs` under `specification/`.
- Specifications are pure — no I/O, no DateTime, no DI, no async. A single `IsSatisfiedBy(...)` method returning bool.
- Services under `service/` are stateless and coordinate logic that does not belong to a single aggregate.
- Invariants are enforced in the aggregate via specifications + `Guard.Against(...)` calls, not inline `if/throw` spaghetti.

### Step 7 — Audit sweep + new-rules capture

Per CLAUDE.md $1b and $1c:

1. Run every `claude/audits/*.audit.md` — most critically [e1-ex-domain.audit.md](../../../audits/e1-ex-domain.audit.md).
2. For every finding, capture under `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` per $1c.
3. Fix all S0 and S1 findings inline before declaring the prompt complete. S2/S3 findings may be captured for follow-up.

## OUTPUT FORMAT

Report at the end of execution:

- **Files created** — grouped by BC, listed by category (aggregate, events, VOs, errors, specs, services).
- **Files modified** — grouped by BC, with one-line change summary each.
- **Pre-flight refactors** — list of aggregates lifted to `AggregateRoot` with before/after pattern.
- **Audit results** — [e1-ex-domain.audit.md](../../../audits/e1-ex-domain.audit.md) pass/fail per rule with evidence (line count, grep result).
- **D-level progression** — for each BC: pre-prompt D-level → post-prompt D-level.
- **New-rules captured** — list of files under `claude/new-rules/` with one-line summary each.
- **Remaining gaps for D2** — sections 7–18 work not done in this prompt (engines, runtime, API, tests, docs).

## VALIDATION CRITERIA

1. [e1-ex-domain.audit.md](../../../audits/e1-ex-domain.audit.md) reports **zero S0 violations** and **zero S1 violations** across the vertical's classification path.
2. Project builds clean: `dotnet build src/domain/` exits 0 with zero warnings / zero errors.
3. Every BC in {{BC_LIST}} reaches at minimum **D1 (Partial)** — sections 1–6 done, sections 7–18 pending.
4. No file outside `src/domain/{{VERTICAL}}-system/**` is modified except `src/domain/shared-kernel/**` if new shared VOs were introduced (must be justified and documented).
5. No new `NotImplementedException`, no new `// TODO`, no new empty method bodies introduced (runtime no-dead-code / stub-detection).
6. Audit sweep + new-rules capture completed per CLAUDE.md $1b / $1c.

## CLASSIFICATION

- Classification: `{{VERTICAL}}` (per DS-R8 — raw classification name without `-system` suffix for `DomainRoute` / `CommandContext`).
- Context: Phase 2.X domain implementation (where X ∈ {5, 6, 7, 8, 9} per [phase-2b.md](../../../project-topics/v2b/phase-2b.md)).
- Domain: {{VERTICAL}}-system domain layer, sections 1–6 of E1 → EX Delivery Pattern v1.0.

## SCOPE EXPLICITLY OUT

- Sections 7–18 (engines, runtime, projections, API, observability, cross-system, tests, docs). Covered by subsequent prompts.
- D2 (Active) promotion. Blocked until sections 7–18 are complete.
- Cross-BC references or integration events. Domain layer is isolated; cross-system wiring happens at the shared-contracts / runtime boundary.
- Policy ID declaration (handled in section 7 — policy integration).
- Projection read models (handled in section 12).
- API DTO naming (handled in section 13).
