# Quality Guard (Canonical)

## Purpose

**Quality** = code integrity + hygiene. This guard consolidates rules governing: clean-code standards, stub / empty-catch / placeholder detection, dead-code elimination, structural code organization (layer purity, dependency direction, classification registry), and prompt-container hygiene. Guard violations are blocking per CLAUDE.md $1a / $1b / $12.

## Source consolidation

This canonical guard merges the following five source guard files (content preserved verbatim or near-verbatim; deduplications noted inline):

- `clean-code.guard.md` → Section: Clean Code Standards
- `stub-detection.guard.md` → Section: Stub Detection
- `no-dead-code.guard.md` → Section: Dead Code Elimination
- `structural.guard.md` → Section: Structural Code Organization
- `prompt-container.guard.md` → Section: Prompt Container Hygiene

Shared WBSM v3 Global Enforcement rules (GE-01..GE-05) appeared verbatim in both `structural.guard.md` and `prompt-container.guard.md`. They have been deduplicated into a single consolidated section: **WBSM v3 Global Enforcement**.

## Mode

MANDATORY — BLOCKING

Enforcement:
- PRE-COMMIT: WARNING
- CI/CD: HARD BLOCK
- RUNTIME: NOT APPLICABLE

## Rules

### Section: Clean Code Standards

_Source: `clean-code.guard.md` (CLEAN CODE GUARD — WBSM v3.5 — LOCKED; classification: governance / clean-code / enforcement)._

Principle: Clean code in Whycespace is defined as:

> Deterministic, readable, domain-aligned, non-over-engineered, and structurally consistent code that is easy to understand, test, and modify.

#### CCG-01 — READABILITY (MANDATORY)

* All variables, methods, classes MUST use descriptive, intention-revealing names
* Single-letter variables are FORBIDDEN (except loop counters)
* Abbreviations are FORBIDDEN unless domain-standard

BLOCK IF:

* ambiguous naming detected
* non-semantic identifiers used

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-02 — FUNCTION SIZE & FOCUS

* Functions MUST do ONE thing only
* Max recommended length: 20–30 lines
* Nested depth MUST NOT exceed 3 levels

BLOCK IF:

* multiple responsibilities detected
* deep nesting (>3)
* large monolithic methods

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-03 — NO SPAGHETTI LOGIC

* Deep nested conditionals MUST be flattened
* Early returns MUST be preferred
* Flow MUST be linear and predictable

BLOCK IF:

* nested if/else chains > 3 levels
* branching chaos without clear flow

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-04 — NO OVER-ENGINEERING

* Do NOT introduce abstractions without clear necessity
* Avoid premature generalization
* Avoid unused interfaces, factories, patterns

BLOCK IF:

* unused abstractions
* speculative architecture
* indirection without value

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-05 — DOMAIN PURITY (CRITICAL)

* Business logic MUST exist ONLY in domain aggregates/entities
* No business logic in:

  * controllers
  * runtime
  * engines (outside orchestration role)

BLOCK IF:

* domain logic leakage detected outside domain layer

**Source:** clean-code.guard.md
**Severity:** CRITICAL (BLOCK)

#### CCG-06 — LAYER ISOLATION

STRICT enforcement:

| Layer    | Allowed Access   |
| -------- | ---------------- |
| Platform | Systems only     |
| Systems  | Runtime only     |
| Runtime  | Engines only     |
| Engines  | Domain only      |
| Domain   | NOTHING external |

BLOCK IF:

* any cross-layer violation occurs

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-07 — DETERMINISM (CRITICAL)

FORBIDDEN:

* Guid.NewGuid()
* DateTime.UtcNow
* Random()

REQUIRED:

* DeterministicIdHelper
* Injected IClock

BLOCK IF:

* non-deterministic behavior detected

**Source:** clean-code.guard.md
**Severity:** CRITICAL (BLOCK)
**Note:** Semantically overlaps with WBSM v3 Global Enforcement GE-01 but retained verbatim because CCG-07 cites concrete helper names (`DeterministicIdHelper`, `IClock`) whereas GE-01 cites abstract interfaces (`IIdGenerator`, `ITimeProvider`). Both preserved.

#### CCG-08 — SELF-DOCUMENTING CODE

* Code MUST express intent without comments
* Comments ONLY allowed for:

  * WHY, not WHAT

BLOCK IF:

* excessive comments explaining obvious logic
* unclear logic requiring explanation

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-09 — CONSISTENCY

* Naming conventions MUST be uniform
* Folder structure MUST follow canonical rules
* DDD structure MUST be complete

BLOCK IF:

* inconsistent naming
* structural deviations

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### CCG-10 — TESTABILITY

* Code MUST be:

  * deterministic
  * side-effect controlled
  * dependency-injected

BLOCK IF:

* hidden dependencies
* untestable logic

**Source:** clean-code.guard.md
**Severity:** MANDATORY (BLOCK)

#### Clean Code Violation Output Format

Violation MUST return:

```
CLEAN_CODE_VIOLATION
Rule: <RULE_ID>
File: <path>
Reason: <description>
Fix: <required correction>
```

---

### Section: Stub Detection

_Source: `stub-detection.guard.md` (Status: ACTIVE; Severity baseline: S0 fails build; S1 requires explicit registry entry; Owner: WBSM v3 structural integrity)._

**Scope:** All files under `src/`. Tests are out of scope.

#### STUB-R1 — Zero NotImplementedException on production path (S0)

`throw new NotImplementedException(...)` is FORBIDDEN in:
- `src/domain/**`
- `src/engines/**`
- `src/runtime/**`
- `src/platform/api/**`
- Anywhere on the Todo E2E path: API → Runtime → Engines → Persistence → Kafka → Response

If a method must be unimplemented, throw a structured domain exception with explicit reason, OR remove the method.

**Source:** stub-detection.guard.md
**Severity:** S0

#### STUB-R2 — Zero TODO/FIXME/HACK on production path (S1)

Comments containing `TODO`, `FIXME`, `HACK`, `XXX` are forbidden in production code. Convert to GitHub issues or new-rules entries instead.

**Source:** stub-detection.guard.md
**Severity:** S1

#### STUB-R3 — Placeholder implementations must be registered (S1)

A class is a "tracked placeholder" only if:
1. Class name begins with `InMemory` OR file/class XML doc contains the literal token `PLACEHOLDER (T-PLACEHOLDER-NN)`
2. There is a corresponding registry entry in `claude/registry/placeholders.json` (or equivalent) with:
   - `id` (matching `T-PLACEHOLDER-NN`)
   - `file`
   - `replacement_target` (e.g., the migration script or canonical implementation path)
   - `phase_gate` (which phase must replace it)
3. Architecture test enforces 1:1 between marker and registry.

Untracked placeholders are S1 violations.

**Source:** stub-detection.guard.md
**Severity:** S1

#### STUB-R4 — No silent exception swallowing (S2)

Forbidden:
```csharp
catch { }
catch (Exception) { }
```
Allowed:
- `catch (OperationCanceledException) { return; }` in shutdown paths
- `catch (SpecificException ex) { _logger.LogDebug(ex, "..."); /* known recoverable */ }`

**Source:** stub-detection.guard.md
**Severity:** S2

#### STUB-R5 — No empty interface implementations without doc (S2)

An empty `void` method implementing an interface contract requires an XML doc comment explaining why it is intentionally a no-op (e.g., "schema-only module owns no engines").

**Source:** stub-detection.guard.md
**Severity:** S2

#### STUB-R6 — No hardcoded placeholder return values (S2)

`return true;`, `return 0;`, `return "ok";`, `return new List<T>();` as the entire method body is forbidden unless the method's contract permits it (verified by name like `IsAlwaysTrue` or interface explicitly states "returns empty when …").

**Source:** stub-detection.guard.md
**Severity:** S2

#### Stub Detection CI Enforcement

1. **Architecture test:** grep `src/{domain,engines,runtime,platform/api}/**/*.cs` for `NotImplementedException` — fail.
2. **Architecture test:** grep `src/**/*.cs` for `\bTODO\b|\bFIXME\b|\bHACK\b|\bXXX\b` outside `// XML doc` — fail.
3. **Architecture test:** for every class matching `^InMemory.*` or comment `PLACEHOLDER \(T-PLACEHOLDER-\d+\)`, assert a registry entry exists in `claude/registry/placeholders.json`.
4. **Architecture test:** scan for `catch\s*\{\s*\}` — fail.

#### Currently Tracked Placeholders

- `T-PLACEHOLDER-01` — InMemoryWorkflowExecutionProjectionStore (replaced by Postgres impl after migration 002)
- `T-PLACEHOLDER-02` — InMemoryStructureRegistry (replaced by canonical constitutional registry)

(Both must be added to `claude/registry/placeholders.json` when that registry is created — see new-rules entry 20260408-132840-stub-detection.md.)

---

### Section: Dead Code Elimination

_Source: `no-dead-code.guard.md` (Status: ACTIVE; Severity baseline: S0 = must remove immediately; S1 = should remove; Owner: WBSM v3 structural integrity)._

**Objective:** Ensure the codebase contains only executable, referenced, and purposeful code. Eliminate misleading, unused, or placeholder artifacts that introduce ambiguity.

**Core Rule:** A file MUST NOT exist in the repository if it has no runtime or compile-time impact, unless it is explicitly allowed under the Exceptions section.

**Canonical Principle:**

> If a developer cannot trace a file to execution, it must not exist.

**Scope:** Applies to `src/domain`, `src/engines`, `src/runtime`, `src/systems`, `src/projections`, `src/platform`, `src/shared`.

**Summary:** Dead code is not neutral—it creates false architecture. This guard ensures: clarity, determinism, maintainability, correct developer understanding.

#### Dead Code Definitions

Code is considered DEAD if ANY of the following are true:

1. It is not referenced anywhere in the codebase
2. It is not part of the build output
3. It is not used by runtime execution (API → Runtime → Engine → Domain → Projection)
4. It is not used by tests
5. It exists only as:

   * a placeholder
   * a redirect stub
   * commented-out logic
   * legacy artifact

#### ND-PROHIBITED — Prohibited Patterns

The following MUST NOT exist:

##### 1. Redirect Stubs

Example:

```csharp
// Moved to ...
public class X {}
```

##### 2. Empty Classes

```csharp
public class TodoService {}
```

##### 3. Unused Interfaces / Implementations

* Interfaces with zero callers
* Implementations not registered or invoked

##### 4. Duplicate Execution Paths

* Old consumers alongside new ones
* Parallel patterns doing the same job

##### 5. Commented-Out Code Blocks

```csharp
// var x = ...
```

**Source:** no-dead-code.guard.md

#### ND-ALLOWED — Allowed Exceptions

The following are allowed:

##### 1. Structural Placeholders

Used to preserve folder structure:

* `.gitkeep`

##### 2. Scaffolding for Future Layers

ONLY if empty and intentional:

* `T3I/`
* `T4A/`

Must contain:

* no logic
* no fake implementations

##### 3. Documentation (Outside Runtime Path)

* `/docs/`
* `/claude/`

NOT allowed inside:

* `/src/`

##### 4. Contracts in Active Use

* DTOs, Commands, Queries even if indirectly referenced

**Source:** no-dead-code.guard.md

#### ND-R1 — Reference Check

Every class must have at least one of:

* direct reference
* DI registration
* runtime invocation
* test usage

**Source:** no-dead-code.guard.md
**Severity:** S0/S1 per Violation Severity table

#### ND-R2 — Registration Check

If a class is meant to execute:

* it MUST be registered (DI / engine registry / workflow registry)

**Source:** no-dead-code.guard.md
**Severity:** S0/S1 per Violation Severity table

#### ND-R3 — Projection Consumption

* No custom consumers if a generic worker exists
* All projections must be reachable via registry

**Source:** no-dead-code.guard.md
**Severity:** S0/S1 per Violation Severity table

#### ND-R4 — Single Pattern Rule

* Only ONE valid implementation pattern per concern

**Source:** no-dead-code.guard.md
**Severity:** S0/S1 per Violation Severity table

#### Dead Code Violation Severity

| Severity | Description                                      |
| -------- | ------------------------------------------------ |
| S0       | Dead code affecting runtime clarity or execution |
| S1       | Unused but harmless code                         |
| S2       | Cosmetic / formatting                            |

#### Dead Code Action on Violation

* S0 → MUST be removed immediately
* S1 → SHOULD be removed
* S2 → optional cleanup

#### Dead Code Examples

Example (Correct):

✔ Used handler registered in engine registry
✔ Projection handler registered in projection registry

Example (Violation):

✘ Old event consumer not used
✘ Empty service class
✘ Stub file with comment "moved to ..."

---

### Section: Structural Code Organization

_Source: `structural.guard.md` (Structural Guard)._

**Purpose:** Enforce repository partition boundaries and layer purity across the WBSM v3 architecture. No layer may reach into another layer's internal structure, and dependency direction must flow inward (platform > systems > runtime > engines > domain).

**Scope:** All files under `src/`. Applies to every commit, PR, and CI pipeline run.

#### STR-01 — DOMAIN ISOLATION

`src/domain/` has ZERO external dependencies. No NuGet packages, no infrastructure imports, no framework references. Domain references only `src/shared/` kernel primitives. No `using` directive may reference any namespace outside `Domain` or `Shared.Kernel`.

**Source:** structural.guard.md

#### STR-02 — SHARED PURITY

`src/shared/` contains ONLY contracts, kernel primitives, and cross-cutting value types. It must NOT contain business logic, domain rules, workflow orchestration, or persistence concerns. Shared may not reference domain, engines, runtime, systems, or platform.

**Source:** structural.guard.md

#### STR-03 — ENGINE TIER COMPLIANCE

`src/engines/` follows the T0U-T4A tiered topology. Engines import from `src/domain/` and `src/shared/` only. Engines NEVER define domain aggregates, entities, value objects, or domain events. Engines never reference other engines, runtime, systems, or platform.

**Source:** structural.guard.md

#### STR-04 — RUNTIME BOUNDARY

`src/runtime/` is the control plane, middleware, and internal projection layer. Runtime may reference engines, domain, and shared. Runtime must NOT contain domain model definitions. Runtime must NOT contain direct persistence logic (only internal projection handlers). Runtime must NOT reference `src/projections/`.

**Source:** structural.guard.md

#### STR-05 — SYSTEMS COMPOSITION

`src/systems/` is composition-only. Systems compose engines via runtime references. Systems must NOT contain execution logic, domain model definitions, or direct persistence. Systems may reference runtime and shared only.

**Source:** structural.guard.md

#### STR-06 — PLATFORM ENTRY

`src/platform/` is the entry layer (API controllers, CLI, host configuration). Platform must NOT reference engines directly. Platform must NOT contain direct database access. Platform references systems and runtime only.

**Source:** structural.guard.md

#### STR-07 — DOMAIN PROJECTIONS LAYER

`src/projections/` is the domain projection layer (CQRS read models / query layer). Projections may reference ONLY `src/shared/` and `infrastructure/` adapters. Projections must NOT reference `src/domain/`, `src/runtime/`, `src/engines/`, `src/systems/`, or `src/platform/`. All domain projections are event-driven only (Kafka/event fabric). Redis/read-store writes originate ONLY from this layer.

**Source:** structural.guard.md

#### STR-08 — INFRASTRUCTURE ADAPTERS

`infrastructure/` contains only adapter implementations (repositories, messaging, external service clients). Infrastructure must NOT contain business logic or domain rules. Infrastructure implements interfaces defined in domain or shared.

**Source:** structural.guard.md

#### STR-09 — DEPENDENCY DIRECTION

Dependencies flow strictly: `platform > systems > runtime > engines > domain < shared`. `src/projections/` is an isolated read-side layer referencing only `shared` and `infrastructure`. No reverse dependency is permitted. No lateral dependency within the same layer (e.g., engine-to-engine). Runtime and projections are mutually isolated — neither may reference the other.

**Source:** structural.guard.md

#### STR-10 — NAMESPACE ALIGNMENT

File namespace must match its physical folder path. A file in `src/domain/economic-system/capital/vault/` must have namespace `Domain.EconomicSystem.Capital.Vault.*`. No namespace aliasing to circumvent layer boundaries.

**Source:** structural.guard.md

#### STR-11 — NO TRANSITIVE LEAKAGE

Public types in inner layers must not expose types from outer layers in their signatures. Domain types must not reference engine or runtime types even transitively.

**Source:** structural.guard.md

#### STR-12 — TEST LAYER MUST MIRROR DOMAIN

The test project structure must mirror the domain structure. For each BC at `src/domain/{system}/{context}/{domain}/`, a corresponding test folder must exist at `tests/{system}/{context}/{domain}/`. Test classes must follow the naming pattern `{ClassName}Tests`. Missing test mirrors for D2-level BCs are a structural violation.

**Source:** structural.guard.md

#### STR-13 — NO BUSINESS LOGIC OUTSIDE DOMAIN

Business rules, domain invariants, and business decision logic must reside exclusively in `src/domain/`. No business logic in engines (engines execute domain operations, not define rules), runtime (runtime orchestrates, not decides), systems (systems compose, not compute), platform (platform routes, not validates), shared (shared carries types, not logic), infrastructure (infrastructure adapts, not reasons), or projections (projections flatten and denormalize, not decide).

**Source:** structural.guard.md

#### STR-14 — EVENT STORE IS SOURCE OF TRUTH

The event store is the canonical source of truth for all domain state. Aggregate state is derived by replaying events. No alternative persistence mechanism may serve as the authoritative state source. Read models, caches, and projections are derived views — never authoritative. If the event store and a projection disagree, the event store is correct.

**Source:** structural.guard.md

#### STR-15 — DUAL PROJECTION MODEL

WBSM v3 enforces two isolated projection layers:
- `src/runtime/projection/` — internal execution support (workflow state, idempotency tracking, policy linking). NOT exposed externally. May be synchronous.
- `src/projections/` — domain read models (CQRS query layer). Event-driven ONLY. The sole query source for APIs. Owns Redis/read-store writes.
Both layers are mandatory, isolated, and non-overlapping. Any cross-reference between them is a CRITICAL violation.

**Source:** structural.guard.md

#### STR-16 — DOMAIN CLASSIFICATION REGISTRY (LOCKED)

The top-level folders under `src/domain/` are the **canonical classification set**. Each classification uses the `{name}-system` suffix. The locked registry is:

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

**Source:** structural.guard.md

#### STR-AUTH-01 — Mutating endpoint authorization (2026-04-07)

Every controller action with HTTP verb POST/PUT/PATCH/DELETE MUST be protected by [Authorize] (controller or action) OR by global authentication middleware registered before MVC routing. A structural scan MUST fail the build for any unprotected mutating endpoint.

**Source:** structural.guard.md (NEW RULES INTEGRATED — 2026-04-07)

#### STR-HEALTH-01 — Health-check layering (2026-04-07)

Health-check adapter implementations live in `src/infrastructure/health/`. Contracts (`IHealthCheck`, `HealthCheckResult`) live in `src/shared/contracts/infrastructure/health/`. HealthAggregator lives in infrastructure and is exposed only via platform API.

**Source:** structural.guard.md (NEW RULES INTEGRATED — 2026-04-07)

#### STR-OBS-01 — Observability middleware location (2026-04-07)

Observability middleware (metrics/logging/tracing) lives ONLY in `src/runtime/observability/`. MUST NOT contain business logic or domain references.

**Source:** structural.guard.md (NEW RULES INTEGRATED — 2026-04-07)

#### STR-GUARD-REGISTRY-01 — Guard-registry drift (2026-04-08)

(S2): CLAUDE.md $1a guard list drifts from on-disk `claude/guards/*.guard.md`. The canonical loading semantics MUST be "load every `*.guard.md` under `/claude/guards/`" rather than an explicit enumeration, so the set self-heals against future guard additions. Any explicit enumeration in CLAUDE.md $1a MUST match the on-disk set exactly, or be replaced with the glob directive. Phase-1 authored guards (clean-code, composition-loader, dependency-graph, determinism, deterministic-id, hash-determinism, platform, program-composition, replay-determinism, runtime-order) are canonical. Source: `claude/new-rules/_archives/20260408-090519-guards.md`.

**Source:** structural.guard.md (NEW RULES INTEGRATED — 2026-04-08)
**Severity:** S2

#### Structural Check Procedure

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

#### Structural Pass Criteria

- All dependency arrows flow in the permitted direction.
- Zero cross-layer import violations detected.
- Zero domain model definitions found outside `src/domain/`.
- Zero business logic found in `src/shared/` or `infrastructure/`.
- All namespaces align with physical folder structure.
- `src/projections/` exists with domain-aligned projection modules.
- `src/runtime/projection/` exists with execution-support projections.
- Zero cross-references between runtime projections and domain projections.
- Domain projections reference only `src/shared/`.

#### Structural Fail Criteria

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

#### Structural Severity Levels

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

#### Structural Enforcement Action

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

### Section: Prompt Container Hygiene

_Source: `prompt-container.guard.md` (Prompt Container Guard)._

**Purpose:** Enforce canonical prompt formatting across all AI-assisted prompts used in the WBSM v3 system. Every prompt must use the markdown container format, declare mandatory sections, avoid broken fencing, support batch execution, and be registered in the prompt registry.

**Scope:** All prompt files (`.prompt.md`, `.prompt.json`, or prompt templates) across the repository, including `claude/` directory prompts, CI prompts, and any prompt used for code generation, auditing, or governance. Evaluated at CI and prompt review.

#### PC-01 — MARKDOWN CONTAINER FORMAT

All prompts must use the standard markdown container structure. Each prompt is a self-contained markdown document with clearly delineated sections using level-2 headings (`##`). No free-form text prompts. No inline prompt strings embedded in code. Every prompt is a file.

**Source:** prompt-container.guard.md

#### PC-02 — MANDATORY SECTIONS

Every prompt must declare these five sections:
- `## Role` — Defines the AI's persona, expertise, and constraints for this prompt.
- `## Objective` — States the specific goal of the prompt in one to three sentences.
- `## Rules` — Numbered list of behavioral rules the AI must follow during execution.
- `## Output` — Defines the expected output format, structure, and delivery method.
- `## Failure` — Defines what constitutes failure, how to detect it, and what to do on failure.
Missing any section makes the prompt non-compliant.

**Source:** prompt-container.guard.md

#### PC-03 — NO BROKEN NESTED CODE FENCING

Prompts that contain code examples must use proper fencing. Triple backticks inside a prompt must not break the outer markdown structure. Use different fence lengths (````` vs ```) or indent-based code blocks when nesting. A prompt with broken fencing is unparseable and therefore invalid.

**Source:** prompt-container.guard.md

#### PC-04 — PROMPTS ARE BATCH-SAFE

Every prompt must be executable in batch mode (non-interactive). Prompts must not require mid-execution user input, confirmations, or interactive decisions. All parameters must be declared upfront in a `## Parameters` section (optional but required if the prompt takes input). Batch-safe means: given inputs, the prompt runs to completion autonomously.

**Source:** prompt-container.guard.md

#### PC-05 — PROMPTS REGISTERED IN PROMPT REGISTRY

Every prompt file must have an entry in `prompt.registry.json` (or equivalent registry file). The registry entry includes: prompt ID, file path, category, version, and last-validated date. Unregistered prompts are not executable by CI or automated systems.

**Source:** prompt-container.guard.md

#### PC-06 — PROMPT VERSIONING

Each prompt must declare its version in a YAML frontmatter block or metadata section. Version follows semver: `major.minor.patch`. Breaking changes to prompt structure increment major. Output format changes increment minor. Clarifications increment patch.

**Source:** prompt-container.guard.md

#### PC-07 — PROMPT CATEGORIZATION

Prompts must be categorized:
- **audit**: Prompts that validate code or architecture.
- **generate**: Prompts that produce code, configs, or artifacts.
- **review**: Prompts that evaluate PRs, diffs, or changes.
- **enforce**: Prompts that check compliance against guards.
- **report**: Prompts that produce summary or status reports.
The category must be declared in the prompt metadata and registry entry.

**Source:** prompt-container.guard.md

#### PC-08 — NO PROMPT INJECTION VECTORS

Prompts must not contain user-controllable interpolation without sanitization. If a prompt accepts parameters, parameter values must be enclosed in delimited blocks (e.g., `<parameter>value</parameter>`) to prevent prompt injection. No raw string concatenation of user input into prompt text.

**Source:** prompt-container.guard.md

#### PC-09 — DETERMINISTIC OUTPUT SPECIFICATION

The `## Output` section must define the exact output format (JSON schema, markdown template, structured report format). Outputs must be machine-parseable when consumed by CI. Free-form prose output is permitted only for human-targeted prompts explicitly marked as such.

**Source:** prompt-container.guard.md

#### PC-10 — PROMPT DEPENDENCY DECLARATION

If a prompt depends on output from another prompt (chained execution), the dependency must be declared in a `## Dependencies` section. The dependency graph must be acyclic. Circular prompt dependencies are forbidden.

**Source:** prompt-container.guard.md

#### PC-11 — PROMPT IDEMPOTENCY

Prompts must be idempotent: running the same prompt with the same inputs on the same codebase state must produce equivalent output. Non-deterministic prompts (e.g., creative generation) must be explicitly marked as `idempotent: false` in metadata.

**Source:** prompt-container.guard.md

#### PC-12 — MAXIMUM PROMPT LENGTH

Individual prompts must not exceed 4000 words. Prompts exceeding this limit must be decomposed into chained sub-prompts with explicit dependency declarations. Overly long prompts degrade AI performance and are harder to audit.

**Source:** prompt-container.guard.md

#### PC-13 — WRITING BLOCK REQUIRED FOR LONG PROMPTS

Prompts that generate or modify more than 5 files, or produce output exceeding 2000 lines, must include a `## Writing Block` section. The writing block declares: target files, expected changes, rollback strategy, and validation criteria. This ensures large-scale prompt executions are auditable and reversible.

**Source:** prompt-container.guard.md

#### PC-14 — NO BROKEN CONTAINERS (CRITICAL)

A prompt container must be syntactically complete. Every opened section must be closed. Every code fence must be balanced. Every parameter placeholder must have a corresponding declaration. A broken container is a prompt that cannot be parsed to completion. Broken containers must fail CI immediately with S0 severity. No partial prompt execution is permitted.

**Source:** prompt-container.guard.md
**Severity:** S0 (CRITICAL)

#### PC-15 — PROMPT MUST DECLARE EXECUTION MODE

Every prompt must declare its execution mode in metadata or frontmatter:
- `mode: autonomous` — prompt runs to completion without human intervention
- `mode: supervised` — prompt pauses at checkpoints for human review
- `mode: dry-run` — prompt simulates execution and reports what would change
Prompts without a declared execution mode default to `supervised` and must be flagged for metadata completion.

**Source:** prompt-container.guard.md

#### PROMPT-RECONCILE-01 — Prompt reconciliation pre-execution (2026-04-07)

(S2): Pasted prompts MUST be reconciled against existing repository surface area BEFORE code emission. For every type / interface / method / path / topic named in the prompt, verify the canonical equivalent in the codebase. If the prompt literal differs from the canonical name (e.g. `IKafkaConsumer` vs `GenericKafkaProjectionConsumerWorker`, `t1m/` vs `T1M/`, `whyce.workflow.execution.events` vs `whyce.orchestration-system.workflow.events`, `IEventStore.LoadAsync` vs `LoadEventsAsync`), the canonical name MUST be used and the divergence recorded inline in the project-prompt file under a RECONCILIATION section. Literal execution of an unreconciled prompt is a $5 anti-drift violation. Source: `claude/new-rules/_archives/20260407-220000-prompt-reconciliation-pre-execution.md`.

**Source:** prompt-container.guard.md (NEW RULES INTEGRATED — 2026-04-07)
**Severity:** S2

#### Prompt Container Check Procedure

1. Enumerate all prompt files matching `*.prompt.md`, `*.prompt.json`, or files in prompt directories.
2. For each prompt file, verify presence of all five mandatory sections: Role, Objective, Rules, Output, Failure.
3. Parse markdown fencing and verify no broken nested code blocks (fence depth tracking).
4. Verify no prompt requires mid-execution user interaction (scan for interactive markers).
5. Verify each prompt has an entry in `prompt.registry.json`.
6. Verify version declaration in frontmatter or metadata for each prompt.
7. Verify category assignment for each prompt.
8. Scan for raw string interpolation patterns (e.g., `${userInput}`, `{0}`) without delimiters.
9. Verify `## Output` section defines structured format (JSON schema, template, or format spec).
10. Build prompt dependency graph and check for cycles.
11. Verify prompt word count does not exceed 4000 words.
12. Cross-reference registry entries against actual prompt files (detect orphaned registrations or unregistered prompts).

#### Prompt Container Pass Criteria

- All prompts use markdown container format.
- All prompts have all five mandatory sections.
- No broken code fencing in any prompt.
- All prompts are batch-safe (no interactive requirements).
- All prompts registered in prompt registry.
- All prompts have version declarations.
- All prompts are categorized.
- No prompt injection vectors detected.
- All output specifications are structured.
- Prompt dependency graph is acyclic.
- All prompts under 4000 words.

#### Prompt Container Fail Criteria

- Prompt missing mandatory section (Role, Objective, Rules, Output, or Failure).
- Broken nested code fencing.
- Prompt requires interactive input mid-execution.
- Prompt not registered in prompt registry.
- Missing version declaration.
- Missing category assignment.
- Raw user input interpolation without sanitization.
- Unstructured output specification for CI-consumed prompt.
- Circular prompt dependency.
- Prompt exceeds 4000 words without decomposition.
- Orphaned registry entry (prompt file deleted but registry entry remains).

#### Prompt Container Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Prompt injection vector | `${userInput}` concatenated into role section |
| **S0 — CRITICAL** | Missing mandatory section | Prompt without `## Failure` section |
| **S1 — HIGH** | Unregistered prompt | Prompt file exists but not in registry |
| **S1 — HIGH** | Broken code fencing | Triple backticks break outer markdown |
| **S1 — HIGH** | Interactive prompt in CI pipeline | Prompt asks "Continue? (y/n)" mid-execution |
| **S2 — MEDIUM** | Missing version | No version in frontmatter or metadata |
| **S2 — MEDIUM** | Circular dependency | Prompt A depends on B, B depends on A |
| **S2 — MEDIUM** | Unstructured output | Output section says "describe the findings" |
| **S3 — LOW** | Missing category | Prompt without category assignment |
| **S3 — LOW** | Prompt over 4000 words | Long prompt without decomposition |
| **S3 — LOW** | Orphaned registry entry | Registry points to deleted prompt file |

#### Prompt Container Enforcement Action

- **S0**: Block merge. Fail CI. Prompt must be fixed or removed immediately.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for cleanup.

All violations produce a structured report:
```
PROMPT_CONTAINER_GUARD_VIOLATION:
  prompt: <prompt file path>
  registry_id: <registry ID if applicable>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  section: <which section is affected>
  expected: <correct format>
  actual: <detected issue>
  remediation: <fix instruction>
```

---

### Section: WBSM v3 Global Enforcement

_Source: appeared identically in both `structural.guard.md` and `prompt-container.guard.md`. Deduplicated here into a single consolidated block. Content preserved verbatim._

#### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

**Source:** structural.guard.md + prompt-container.guard.md (deduplicated)

#### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

**Source:** structural.guard.md + prompt-container.guard.md (deduplicated)

#### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

**Source:** structural.guard.md + prompt-container.guard.md (deduplicated)

#### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

**Source:** structural.guard.md + prompt-container.guard.md (deduplicated)

#### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

**Source:** structural.guard.md + prompt-container.guard.md (deduplicated)

---

## Lock status

LOCKED — CANONICAL
