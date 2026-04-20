# Economic-System VO Hardening + E1 → EX Delivery Pattern Template

## TITLE

Phase 2 Closure Pass — Economic Core 2% Hardening and Reusable Engine-Delivery Template

## CONTEXT

Phase-2 audit (see prior conversation) confirmed the Economic Core is ~98% complete, production-grade across 12 bounded contexts, with 40 aggregates / 160 events / 41 projection handlers / 42 controllers / 107 tests, and zero red-flag patterns (no `NotImplementedException`, no `// TODO`, no scaffolded returns).

A deeper inspection at the start of this prompt revealed the audit's "remaining 2%" findings were imprecise:

1. **Money / Currency / Amount / ExchangeRate / Percentage value objects already exist** under `src/domain/shared-kernel/primitive/money/`. They are 3-line skeletal `record struct`s with no validation; not "absent" — under-built.
2. **`src/runtime/context/EconomicContextResolver.cs` is dead code.** Grep confirms zero callers across the entire repository. Its existence is itself a `runtime.guard.md` no-dead-code violation.
3. **`EntryController` read-only is by design** — projection-driven side-effect of journal posts, not a defect. Out of scope.

Separately, Phase 2 item #4 (E1 → EX reusable delivery pattern standardization) is **Not Started** per the audit. The economic system has now proven the canonical pattern end-to-end: `Domain → Engine (T1M Steps + Workflows) → Runtime (dispatcher + middleware) → Composition Root → Platform API → Projections → Tests`. Extracting that proven pattern into a reusable template is the highest-leverage move to accelerate Phase 2.5 (Structural), 2.6 (Content), 2.8 (WhyceID), and 2.9 (WhyceChain) builds.

## OBJECTIVE

Close the genuine "remaining 2%" of the Economic Core and produce the canonical E1 → EX delivery pattern template that subsequent vertical-system builds will follow.

## CONSTRAINTS

- **CLAUDE.md $1a** — Load all four canonical guards (`constitutional`, `runtime`, `domain`, `infrastructure`) before any code change. Treat their rules as halting constraints.
- **CLAUDE.md $5 Anti-Drift** — No new architecture, no renaming of canonical types, no file moves outside the necessary deletions/additions for this prompt. Do not migrate existing economic events to use Money VO in this pass — that is a separate, larger initiative requiring schema migration + replay regression tests per `INV-REPLAY-LOSSLESS-VALUEOBJECT-01`.
- **CLAUDE.md $7 Layer Purity** — Money VOs live in `src/domain/shared-kernel/primitive/money/`. The canonical 11-system table in `domain.guard.md` lists `money` as a shared-kernel primitive — that placement is correct and immutable.
- **`domain.guard.md` rule 5** — Value objects must be immutable. Use `readonly record struct` with `{ get; init; }` semantics only.
- **`domain.guard.md` D-ERR-TYPING-01** — Domain code MUST NOT throw BCL exceptions directly. Use `Guard.Against(...)` from shared-kernel or domain-specific error factories. Money VO validation MUST follow this rule.
- **`constitutional.guard.md` G5** — Domain code MUST NOT call `IDeterministicIdEngine`. Money/Currency/Amount have no identity needs.
- **`constitutional.guard.md` Hash Determinism** — Money equality and hashing must be deterministic (record struct semantics already guarantee this). No floating-point reliance — `decimal` only.
- **`runtime.guard.md` no-dead-code** — Confirmed dead code MUST be deleted, not preserved "in case it's needed". `EconomicContextResolver` qualifies.
- **`INV-REPLAY-LOSSLESS-VALUEOBJECT-01`** — `EventDeserializer.StoredOptions` already registers `AmountJsonConverter` and `CurrencyJsonConverter`, plus the `WrappedPrimitiveValueObjectConverterFactory` covering single-primitive wrappers. **No converter changes are required by this prompt.** Money (two-property: Amount + Currency) currently has no event-payload usage so does not yet require a converter; if Money is later adopted in events, a converter must be added in the same commit per the canonical pattern.

## EXECUTION STEPS

### Step 1 — Delete dead `EconomicContextResolver`

- Verify zero callers via grep across `src/`, `tests/`, `infrastructure/`, `scripts/`.
- Delete `src/runtime/context/EconomicContextResolver.cs`.
- Verify build still passes (the file should not be referenced by any composition root).

### Step 2 — Strengthen Money primitive value objects

For each of `Currency.cs`, `Amount.cs`, `Percentage.cs`, `ExchangeRate.cs`, `Money.cs` under `src/domain/shared-kernel/primitive/money/`:

- Keep the canonical `readonly record struct` shape — back-compatibility with the wrapper-struct converter factory depends on this.
- Add validation in the constructor using `Guard.Against(...)` (or domain-specific shared-kernel error factory if one already exists) per `D-ERR-TYPING-01`. Examples of invariants to enforce:
  - `Currency.Code` non-null, non-empty, exactly 3 chars, ISO-4217 uppercase.
  - `Amount.Value` not `decimal.MinValue` / `decimal.MaxValue` (sentinel rejection); allow negative for now since accounting requires it.
  - `Percentage.Value` in `[0, 1]` if used as ratio, or document otherwise.
  - `ExchangeRate.Rate > 0` and `From != To`.
  - `Money(Amount, Currency)` requires both to be valid (struct constructors compose validation automatically).
- Preserve the single canonical primitive property name (`Value` for Amount/Percentage, `Code` for Currency) so `WrappedPrimitiveValueObjectConverterFactory` continues to match.
- Do NOT change namespace, file location, or type kind. Anti-drift constraint.

### Step 3 — E1 → EX delivery pattern template

Create the canonical reusable engine-delivery template under `claude/templates/e1-ex-delivery-pattern/`:

- `README.md` — overview of the pattern, when to apply it, the 18-section vertical-system structure derived from `phase-2b.md`, and the proven economic exemplar paths.
- `00-section-checklist.md` — the 18 ordered sections every vertical system must implement (System Definition → Core Model → Identity → Domain Implementation → Aggregate Design → Invariants → Policy Integration → Runtime Integration → App/Engine Services → Persistence → Messaging → Projections → API → Observability → Cross-System Integration → Tests → Resilience → Docs).
- `01-domain-skeleton.md` — folder layout, mandatory artifact subfolders, naming rules, with a worked example pointing at a representative economic context (e.g., `economic-system/capital/account/`).
- `02-engine-skeleton.md` — T1M domain layout (Steps + Workflows + Pipeline + Handlers), referencing `src/engines/T1M/domains/economic/transaction/` as the canonical exemplar.
- `03-runtime-wiring.md` — dispatcher registration, middleware enforcement, composition root entries, with the economic exemplar.
- `04-api-projection-tests.md` — controller pattern, projection reducer + handler pattern, three-tier test pattern (unit / integration / e2e) with file counts as quality bars.
- `05-quality-gates.md` — the audit checklist a new vertical system must pass before being declared D2 (Active): zero `NotImplementedException`, all bounded contexts mapped to projections, all aggregates have replay regression tests if they declare wrapper-struct VOs, all commands routed through runtime middleware, etc.

The template is documentation-only in this pass. A future pass may add a `dotnet new` template or scaffolding script.

### Step 4 — Audit sweep per $1b

Run all `claude/audits/*.audit.md` against the changes. Capture any new violations under `claude/new-rules/{YYYYMMDD-HHMMSS}-{type}.md` per $1c.

## OUTPUT FORMAT

- Modified files (with diffs/edits):
  - `src/domain/shared-kernel/primitive/money/Currency.cs`
  - `src/domain/shared-kernel/primitive/money/Amount.cs`
  - `src/domain/shared-kernel/primitive/money/Percentage.cs`
  - `src/domain/shared-kernel/primitive/money/ExchangeRate.cs`
  - `src/domain/shared-kernel/primitive/money/Money.cs`
- Deleted file:
  - `src/runtime/context/EconomicContextResolver.cs`
- New files:
  - `claude/templates/e1-ex-delivery-pattern/README.md`
  - `claude/templates/e1-ex-delivery-pattern/00-section-checklist.md`
  - `claude/templates/e1-ex-delivery-pattern/01-domain-skeleton.md`
  - `claude/templates/e1-ex-delivery-pattern/02-engine-skeleton.md`
  - `claude/templates/e1-ex-delivery-pattern/03-runtime-wiring.md`
  - `claude/templates/e1-ex-delivery-pattern/04-api-projection-tests.md`
  - `claude/templates/e1-ex-delivery-pattern/05-quality-gates.md`
- Audit sweep report appended at the end of the conversation.

## VALIDATION CRITERIA

1. Project builds (no broken references after `EconomicContextResolver` deletion).
2. No domain VO throws BCL exception types directly (D-ERR-TYPING-01).
3. Money/Currency/Amount/Percentage/ExchangeRate remain `readonly record struct` with `{ get; init; }` semantics (rule 5).
4. `WrappedPrimitiveValueObjectConverterFactory` still resolves Currency/Amount/Percentage by single-primitive property name (`Value` / `Code`).
5. No `Guid.NewGuid()`, `DateTime.*`, `Random` introduced anywhere in the diff (GE-01 / DET-01).
6. Template directory contains all six files listed above; each cross-references a real exemplar path that exists in the repo.
7. Phase-2 audit can be re-run with the dead-code item resolved and the E1 → EX item moved from ⚪ to ✅ on the documentation track.

## CLASSIFICATION

- Classification: `economic-system` + `runtime` + `claude-orchestration`
- Context: Phase 2 closure
- Domain: shared-kernel money primitives, runtime context dead-code cleanup, claude templates

## SCOPE EXPLICITLY OUT

- Migrating existing economic commands/events to use Money/Currency/Amount VOs (large schema-migration + replay-regression initiative; deferred).
- Refactoring EntryController (by-design, not a defect).
- Constructing a new context resolver to replace the deleted dead code (no callers means no consumer needs replacement).
- Code-generation / `dotnet new` scaffolds for the E1 → EX template (documentation pass only).
