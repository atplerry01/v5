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
- Allowed references: `systems` only
- Forbidden: `runtime`, `engines`, `domain`, `projections` (direct)
- NOTE: composition root may require exceptions; any exception MUST be
  declared explicitly in this guard. None are currently granted.

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
| platform/host  | src/platform/host | systems, shared                               |

Any `<ProjectReference>` outside this matrix = VIOLATION.

---

## CODE-LEVEL CHECKS

Run on every execution:

```
grep -r "using .*Runtime"        src/domain
grep -r "using .*Infrastructure" src/engines
grep -r "using .*Engines"        src/systems
grep -r "using .*Runtime"        src/platform
grep -r "using .*Engines"        src/projections
grep -r "using .*Runtime"        src/projections
```

Any hit = VIOLATION.

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
1. All rules R1–R7 pass
2. No illegal project references
3. No circular dependencies
4. CI runs `scripts/dependency-check.sh`
5. dependency-graph.audit.md reports FULL PASS
---

## NEW RULES INTEGRATED — 2026-04-07

- **DG-R7-01**: ~~Pre-existing violation tracked: Whycespace.Projections.csproj references Whycespace.Runtime.csproj~~ → **REMEDIATED 2026-04-07**. Resolution: introduced shared `IEnvelopeProjectionHandler` and `IEventEnvelope` contracts under `src/shared/contracts/projection/` and `src/shared/contracts/event-fabric/`. Runtime `EventEnvelope` record now implements `IEventEnvelope`; projection handlers in `src/projections/**` consume the shared contracts only. The runtime project reference has been removed from `Whycespace.Projections.csproj`. Verified by `dotnet build` green across host, unit, and integration projects.
- **DG-R5-01**: ~~Pre-existing violation tracked~~ → **CONVERTED TO DOCUMENTED EXCEPTION (2026-04-07)**. See DG-R5-EXCEPT-01 below.

## EXCEPTIONS (documented and granted)

### DG-R5-EXCEPT-01 — Composition Root references (2026-04-07)

`src/platform/host/Whycespace.Host.csproj` MAY reference `Whycespace.Runtime`,
`Whycespace.Engines`, `Whycespace.Projections`, `Whycespace.Domain`, and
infrastructure adapters **for DI registration purposes only**.

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
remains the sole outstanding tracked violation under this guard, pending
the `IProjectionHandler` relocation in Prompt B Step B-1.
