# DEPENDENCY GRAPH GUARD

## CLASSIFICATION
system / governance / dependency-control

## PRIORITY
S0 — Architectural Safety. Violations HALT execution.

## SCOPE
Entire repository. Loaded fresh on every prompt execution per CLAUDE.md $1a.

---

## CANONICAL LAYER ORDER (TOP → BOTTOM)

```
platform
  ↓
systems
  ↓
runtime
  ↓
engines
  ↓
domain
  ↓
shared
```

A layer MAY only reference layers strictly below it, subject to the rules
below. Upward references are FORBIDDEN. Cycles are FORBIDDEN.

---

## RULES

### R1 — DOMAIN PURITY
- Path: `src/domain/**`
- Allowed references: `shared` only
- Forbidden: `engines`, `runtime`, `systems`, `platform`, `infrastructure`,
  `projections`
- Failure → S0 halt

### R2 — ENGINE PURITY
- Path: `src/engines/**`
- Allowed references: `domain`, `shared`
- Forbidden: `runtime`, `systems`, `platform`, `infrastructure`, `projections`

### R3 — RUNTIME AUTHORITY
- Path: `src/runtime/**`
- Allowed references: `engines`, `domain`, `shared`
- Forbidden: `systems`, `platform`, `projections`

### R4 — SYSTEMS BOUNDARY
- Path: `src/systems/**`
- Allowed references: `runtime` (contracts/interfaces ONLY), `shared`
- Forbidden: `engines` (direct), `infrastructure`, `platform`, `projections`

### R5 — PLATFORM ISOLATION
- Path: `src/platform/**`
- Default rule: `src/platform/api` may reference `systems` and `shared`
  only. Forbidden in `platform/api`: `runtime`, `engines`, `domain`,
  `projections`.
- **Composition-root exception:** `src/platform/host` is the composition
  root and is governed by **DG-R5-EXCEPT-01** (lines below). It MAY
  reference `runtime`, `engines`, `systems`, `projections`, and
  infrastructure adapters for DI registration purposes only. It MUST
  NOT reference `Whycespace.Domain` (see **DG-R5-HOST-DOMAIN-FORBIDDEN**).
- See DG-R5-EXCEPT-01 and DG-R5-HOST-DOMAIN-FORBIDDEN in the EXCEPTIONS
  section below for the canonical wording. Authoritative mechanical
  enforcement is `scripts/dependency-check.sh`.

### R6 — INFRASTRUCTURE RULE
- Path: `src/infrastructure/**` (when present)
- Implements `runtime` interfaces ONLY
- Forbidden: referenced by `domain` or `engines`

### R7 — PROJECTION RULE
- Path: `src/projections/**`
- Allowed references: `domain` (events only), `shared`
- Forbidden: `engines`, `runtime`, `systems`, `platform`
- Projections MUST NOT contain domain logic

---

## ENFORCEMENT MAPPING

| Layer          | Path              | Allowed Project Refs                          |
|----------------|-------------------|-----------------------------------------------|
| shared         | src/shared        | (none)                                        |
| domain         | src/domain        | shared                                        |
| engines        | src/engines       | domain, shared                                |
| runtime        | src/runtime       | engines, domain, shared                       |
| systems        | src/systems       | runtime (contracts), shared                   |
| projections    | src/projections   | domain, shared                                |
| platform/api   | src/platform/api  | systems, shared                               |
| platform/host  | src/platform/host | systems, shared, api, runtime, engines, projections (DI-only, DG-R5-EXCEPT-01; **NOT domain** per DG-R5-HOST-DOMAIN-FORBIDDEN) |

Any `<ProjectReference>` outside this matrix = VIOLATION.

---

## CODE-LEVEL CHECKS

**Authoritative enforcement:** `scripts/dependency-check.sh`. The
script is the single source of truth for the mechanical predicates;
the patterns below are illustrative summaries that must agree with
the script and with DG-R5-EXCEPT-01 / DG-R5-HOST-DOMAIN-FORBIDDEN.

Illustrative predicates run on every execution:

```
grep -r "using .*Runtime"        src/domain
grep -r "using .*Infrastructure" src/engines
grep -r "using .*Engines"        src/systems
grep -r "using .*Runtime"        src/platform/api      # platform/api only
grep -r "using .*Engines"        src/projections
grep -r "using .*Runtime"        src/projections
grep -r "Whycespace\.Domain\."   src/platform/host     # DG-R5-HOST-DOMAIN-FORBIDDEN
```

`src/platform/host` is intentionally omitted from the runtime/engines
grep above: composition-root usings are permitted under
DG-R5-EXCEPT-01. Only `Whycespace.Domain.*` references in
`src/platform/host/**` are forbidden, and the script enforces all
three forms (using directive, fully-qualified expression, namespace
alias) per the §5.1.2 Step C-G strengthening.

Any hit = VIOLATION (subject to documented exceptions below).

---

## ADAPTER LEAKAGE

Adapters are permitted ONLY in:
- `src/platform/host/adapters/**`
- `src/infrastructure/**`

Any type/file matching `*Adapter*` outside those paths = VIOLATION.

---

## SHARED KERNEL

`src/shared/**` MUST contain only:
- primitives
- contracts
- interfaces

FORBIDDEN inside shared:
- runtime logic
- infrastructure logic
- engine logic

---

## FAILURE ACTION

On any violation:
1. HALT execution (CLAUDE.md $12)
2. Emit structured failure: STATUS / STAGE=GUARD / REASON / ACTION_REQUIRED
3. Do NOT auto-fix architecture (CLAUDE.md $5). Report and require explicit
   prompt for remediation.

---

## LOCK CONDITIONS

Guard is LOCKED only if:
1. All rules R1–R7 pass, **and** all DG-* additions pass:
   `DG-R5-EXCEPT-01` (composition-root permission, narrowed 2026-04-08),
   `DG-R5-HOST-DOMAIN-FORBIDDEN` (host→domain prohibition, strengthened
   §5.1.2 Step C-G), and `DG-R7-01` (projections→runtime, remediated
   2026-04-07).
2. No illegal project references
3. No circular dependencies
4. CI runs `scripts/dependency-check.sh`
5. dependency-graph.audit.md reports FULL PASS
---

## NEW RULES INTEGRATED — 2026-04-07

- **DG-R7-01**: ~~Pre-existing violation tracked: Whycespace.Projections.csproj references Whycespace.Runtime.csproj~~ → **REMEDIATED 2026-04-07**. Resolution: introduced shared `IEnvelopeProjectionHandler` and `IEventEnvelope` contracts under `src/shared/contracts/projection/` and `src/shared/contracts/event-fabric/`. Runtime `EventEnvelope` record now implements `IEventEnvelope`; projection handlers in `src/projections/**` consume the shared contracts only. The runtime project reference has been removed from `Whycespace.Projections.csproj`. Verified by `dotnet build` green across host, unit, and integration projects.
- **DG-R5-01**: ~~Pre-existing violation tracked~~ → **CONVERTED TO DOCUMENTED EXCEPTION (2026-04-07)**. See DG-R5-EXCEPT-01 below.

## EXCEPTIONS (documented and granted)

### DG-R5-EXCEPT-01 — Composition Root references (2026-04-07; narrowed 2026-04-08)

`src/platform/host/Whycespace.Host.csproj` MAY reference `Whycespace.Runtime`,
`Whycespace.Engines`, `Whycespace.Systems`, `Whycespace.Projections`, and
infrastructure adapters **for DI registration purposes only**.

`Whycespace.Domain` is **NOT** in the permitted list. Per Phase 1.5 §5.1.1
Step C (2026-04-08), the sole residual host → domain typed usage in
`src/platform/host/adapters/PostgresOutboxAdapter.cs` was replaced with a
reflection-based `.Value` unwrap, and the `<ProjectReference>` to
`Whycespace.Domain.csproj` was removed from `Whycespace.Host.csproj`.
Re-introducing either the csproj reference or any `using Whycespace.Domain.*`
inside `src/platform/host/**` is a fresh S0 violation under R5 and
**DG-R5-HOST-DOMAIN-FORBIDDEN** (see below).

**Authority:** This exception aligns with the already-canonical composition-root
permission in [platform.guard.md G-PLATFORM-07](platform.guard.md):

> "Host (`Program.cs`) is the composition root and MAY reference runtime,
> engines, systems, domain, and infrastructure for DI registration purposes
> only."

The prior R5 wording ("Allowed: systems only") was inconsistent with
G-PLATFORM-07 and produced a perpetually-tracked violation that was
not actually a violation under the canonical platform guard. This
exception entry resolves the inconsistency by recording the DI-only
permission explicitly inside dependency-graph.guard.md.

**Constraints on the exception:**
1. The references are permitted **only** in `Whycespace.Host.csproj` itself.
   No other project under `src/platform/**` may use this exception.
2. The references must be **DI registration only**. Per
   [program-composition.guard.md G-PROGCOMP-01 / G-PROGCOMP-03](program-composition.guard.md),
   `Program.cs` must not contain `AddSingleton<...>` calls keyed on
   concrete domain types — domain wiring flows through
   `IDomainBootstrapModule` and `BootstrapModuleCatalog`, and category
   wiring flows through the literal-list `CompositionModuleLoader`.
3. Per [runtime.guard.md rule 11.R-DOM-01](runtime.guard.md), the host
   may not contain folders nested by `{classification}/{context}/{domain}/`
   nor hold static dictionaries keyed on a single domain.
4. Removal of any of these direct references is permitted only after
   the corresponding wiring has been migrated into a bootstrap module
   listed in `BootstrapModuleCatalog` or a category composition module.

**LOCK status:** With this exception logged, R5 is no longer suspended
on `Whycespace.Host.csproj` references. DG-R7-01 (projections → runtime)
was remediated 2026-04-07. Phase 1.5 §5.1.1 Step C (2026-04-08) removed
the `host → domain` csproj edge. No outstanding tracked violations remain
under this guard pending verification by a clean full-solution build and
a green `scripts/dependency-check.sh` run.

---

### DG-R5-HOST-DOMAIN-FORBIDDEN — host may not depend on domain (2026-04-08)

**Rule:** `src/platform/host/**` may not, under any circumstance:
1. Declare `<ProjectReference Include="..\..\domain\Whycespace.Domain.csproj" />`
   in `Whycespace.Host.csproj`.
2. Contain `using Whycespace.Domain.*;` in any `*.cs` file.
3. Contain a **fully-qualified type expression** referencing
   `Whycespace.Domain.*` — including but not limited to
   `typeof(Whycespace.Domain.X.Y)`, parameter types, generic arguments,
   cast expressions `(Whycespace.Domain.X.Y)e`, and field/property type
   declarations.
4. Contain a **namespace alias** of the form
   `using <Alias> = Whycespace.Domain.<…>;` (e.g.
   `using DomainEvents = Whycespace.Domain.OrchestrationSystem.Workflow.Execution;`).
5. Re-introduce a typed dependency on any `Whycespace.Domain.SharedKernel.*`
   primitive. Adapters that need to inspect domain event shapes MUST do so
   via reflection or via shared contracts under `src/shared/contracts/**`.

Clauses 3 and 4 were added under Phase 1.5 §5.1.2 Step C-G after BPV-D01
exposed eleven live binding sites that bypassed clause 2 by using
fully-qualified or aliased forms. The strengthened predicate is enforced
mechanically by `scripts/dependency-check.sh` (see the `host_fq_hits`
block immediately following the C2 scan).

**Severity:** S0. Violations HALT execution and fail
`scripts/dependency-check.sh`.

**Authority:** Phase 1.5 §5.1.1 Step C remediation. The composition root
must wire modules and own infrastructure adapters; it must not import
domain primitives directly. Domain reachability remains available
transitively via `runtime → domain` for type identity at runtime, but
no host source file may bind to a domain symbol at compile time.

## NEW RULES INTEGRATED — 2026-04-07 (baseline scan addendum)

- **DG-BASELINE-01** (S0): Dual violations logged — (R7) Projections →
  Runtime and (R5 ×4) Platform/Host fan-in to Runtime/Engines/Projections/
  Domain. LOCK blocked until remediated (invert dependencies via shared/
  domain contracts and route Host composition through systems facades) OR
  narrow exception documented inline. See
  `claude/new-rules/_archives/20260407-160000-dependency-graph.md`.
