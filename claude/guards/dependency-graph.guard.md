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

- **DG-R7-01**: Pre-existing violation tracked: Whycespace.Projections.csproj references Whycespace.Runtime.csproj. Projections may reference ONLY domain (events) + shared. Must be remediated OR granted a narrow, documented exception (interface-only, no implementation types).
- **DG-R5-01**: Pre-existing violation tracked: Whycespace.Host.csproj references Runtime, Engines, Projections, Domain directly. Under R5 platform may reference only systems (+shared). Composition root may warrant a documented exception, but none currently granted. LOCK is suspended until remediated or exception logged here.
