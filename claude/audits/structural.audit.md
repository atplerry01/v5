# STRUCTURAL AUDIT — WBSM v3

```
AUDIT ID:       STRUCT-AUDIT-v3
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
SUPERSEDES:     STRUCT-AUDIT-v1 (REV 1)
```

## PURPOSE

Audit the entire repository structure against the WBSM v3 canonical architecture to detect structural violations, layer leakage, incorrect file placement, dependency direction violations, namespace misalignment, and drift from the canonical folder topology.

This audit enforces:

* Layer boundary integrity (domain/shared/engines/runtime/systems/platform/infrastructure)
* Folder topology compliance
* Naming conventions
* Dependency direction (no upward imports)
* File placement correctness
* Namespace alignment with folder structure
* No flat domain structures

---

## SCOPE

```
src/domain/          -> domain model topology
src/shared/          -> contracts, kernel, primitives
src/engines/         -> tiered engine structure (T0U-T4A)
src/runtime/         -> control plane, middleware, internal projection
src/projections/     -> domain projections (read models / CQRS query layer)
src/systems/         -> upstream/midstream/downstream composition
src/platform/        -> entry layer
infrastructure/      -> adapters only
```

Excluded: `bin/`, `obj/`, `.git/`, `.vs/`, `node_modules/`, `tests/`

---

## CANONICAL REPOSITORY STRUCTURE

```
src/
├── domain/
│   └── {classification}/
│       └── {context}/
│           └── {domain}/
│               ├── aggregate/
│               ├── entity/
│               ├── value-object/
│               ├── event/
│               ├── error/
│               ├── service/
│               └── specification/
├── engines/
│   ├── T0U/
│   ├── T1M/
│   ├── T2E/
│   ├── T3I/
│   └── T4A/
├── runtime/
│   └── projection/       # INTERNAL runtime projections (execution support ONLY)
├── projections/           # DOMAIN projections (read models / CQRS query layer)
├── systems/
│   ├── upstream/
│   ├── midstream/
│   │   ├── wss/
│   │   ├── heos/
│   │   └── whyceatlas/
│   └── downstream/
├── platform/
├── shared/
infrastructure/
```

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Layer boundary violation, upward dependency, domain corruption | Blocks deployment |
| HIGH | Missing canonical folder, namespace misalignment | Must fix before merge |
| MEDIUM | Naming convention violation, non-standard file placement | Fix within sprint |
| LOW | Missing .gitkeep, cosmetic folder deviation | Fix at convenience |

---

## GLOBAL RULE: PROJECTION LAYER AUTHORITY

* `src/projections/` = DOMAIN PROJECTION LAYER (CQRS READ MODELS)
* `src/runtime/projection/` = INTERNAL EXECUTION SUPPORT ONLY

MANDATORY:

* Domain projections:
  * consume EVENTS only
  * produce READ MODELS only
  * exposed via platform APIs
* Runtime projections:
  * NOT exposed externally
  * support execution only (routing, orchestration, indexing)

> **Phase 1 requires a fully connected execution path from Platform → Systems → Runtime → Engines → Domain → Events → Projections.**

---

## AUDIT DIMENSIONS

### SDIM-01: Layer Boundary Integrity

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | `src/domain/` exists as top-level domain layer | CRITICAL |
| CHECK-01.2 | `src/shared/` exists as top-level shared layer | CRITICAL |
| CHECK-01.3 | `src/engines/` exists with T0U/T1M/T2E/T3I/T4A subdirectories | CRITICAL |
| CHECK-01.4 | `src/runtime/` exists as top-level runtime layer | CRITICAL |
| CHECK-01.5 | `src/systems/` exists with upstream/midstream/downstream | CRITICAL |
| CHECK-01.6 | `src/platform/` exists as top-level entry layer | CRITICAL |
| CHECK-01.7 | `infrastructure/` exists as top-level adapter layer | CRITICAL |
| CHECK-01.8 | `src/projections/` exists as domain projection layer (CQRS read models) | CRITICAL |
| CHECK-01.9 | `src/runtime/projection/` exists as internal runtime projection layer | CRITICAL |

### SDIM-02: Domain Folder Topology

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Every BC follows CLASSIFICATION > CONTEXT > DOMAIN three-level nesting | CRITICAL |
| CHECK-02.2 | No flat domain structures (domain/{bc}/ without classification) | CRITICAL |
| CHECK-02.3 | Each domain folder contains required DDD subdirectories (aggregate/entity/value-object/event/error/service/specification) | HIGH |
| CHECK-02.4 | No files placed directly in domain/{classification}/ (must be in context/domain/) | HIGH |

### SDIM-03: Naming Conventions

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | All folder names use lowercase kebab-case or lowercase | MEDIUM |
| CHECK-03.2 | All C# files use PascalCase naming | MEDIUM |
| CHECK-03.3 | Aggregate files end with aggregate root name (e.g., `Cluster.cs`) | MEDIUM |
| CHECK-03.4 | Event files end with `Event.cs` suffix | MEDIUM |
| CHECK-03.5 | Error files end with `Error.cs` or `Errors.cs` suffix | MEDIUM |
| CHECK-03.6 | Specification files end with `Spec.cs` or `Specification.cs` suffix | MEDIUM |

### SDIM-04: Dependency Direction (No Upward Imports)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Domain has ZERO imports from engines, runtime, systems, platform, infrastructure | CRITICAL |
| CHECK-04.2 | Shared has ZERO imports from domain, engines, runtime, systems, platform | CRITICAL |
| CHECK-04.3 | Engines do not import from runtime, systems, or platform | CRITICAL |
| CHECK-04.4 | Runtime does not import from systems or platform | CRITICAL |
| CHECK-04.5 | Systems do not import from platform | HIGH |
| CHECK-04.6 | Infrastructure does not import from platform or systems | HIGH |
| CHECK-04.7 | `src/projections/` does not import from domain, runtime, engines, systems, or platform | CRITICAL |
| CHECK-04.8 | `src/runtime/projection/` does not import from `src/projections/` | CRITICAL |
| CHECK-04.9 | `src/projections/` references ONLY `src/shared/` (project references) | CRITICAL |

### SDIM-05: File Placement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | Aggregates are only in `aggregate/` folders | HIGH |
| CHECK-05.2 | Entities are only in `entity/` folders | HIGH |
| CHECK-05.3 | Value objects are only in `value-object/` folders | HIGH |
| CHECK-05.4 | Domain events are only in `event/` folders | HIGH |
| CHECK-05.5 | Domain errors are only in `error/` folders | HIGH |
| CHECK-05.6 | Domain services are only in `service/` folders | HIGH |
| CHECK-05.7 | Specifications are only in `specification/` folders | HIGH |

### SDIM-06: Namespace Alignment

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | C# namespace matches folder path (e.g., `Whycespace.Domain.Clusters.Structure.Cluster`) | HIGH |
| CHECK-06.2 | No namespace collisions across layers | HIGH |
| CHECK-06.3 | Shared contracts use `Whycespace.Shared.*` namespace | MEDIUM |

### SDIM-07: Engine Structure

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Only T0U/T1M/T2E/T3I/T4A tier directories exist under engines | CRITICAL |
| CHECK-07.2 | No engine files placed outside tier directories | HIGH |
| CHECK-07.3 | Engine tier folders contain only engine-scoped artifacts | MEDIUM |

### SDIM-08: Systems Structure

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | Systems contains only upstream/midstream/downstream | HIGH |
| CHECK-08.2 | Midstream contains WSS, HEOS, WhyceAtlas subsystems | HIGH |
| CHECK-08.3 | No execution logic files in systems layer | CRITICAL |

### SDIM-09: Platform Structure

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Platform contains only entry-point configurations | HIGH |
| CHECK-09.2 | No engine references in platform layer | CRITICAL |
| CHECK-09.3 | No direct database access in platform | CRITICAL |

### SDIM-10: Projection Layer Separation (S24)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | `src/runtime/projection/` MUST exist and contain ONLY execution-support projections | CRITICAL |
| CHECK-10.2 | `src/projections/` MUST exist and contain ONLY domain read models | CRITICAL |
| CHECK-10.3 | `src/runtime/` MUST NOT reference `src/projections/` | CRITICAL |
| CHECK-10.4 | `src/projections/` MUST NOT reference `src/runtime/` | CRITICAL |
| CHECK-10.5 | `src/projections/` MUST NOT reference `src/domain/` | CRITICAL |
| CHECK-10.6 | `src/projections/` MUST NOT reference `src/engines/` | CRITICAL |
| CHECK-10.7 | Runtime projections MUST NOT be exposed via API layer | CRITICAL |
| CHECK-10.8 | Domain projections MUST consume events ONLY (no direct method invocation) | HIGH |
| CHECK-10.9 | Redis/read-store writes originate ONLY from `src/projections/` | HIGH |
| CHECK-10.10 | Runtime projections MUST NOT write to Redis | HIGH |
| CHECK-10.11 | All projection handlers are idempotent and replay-safe | HIGH |
| CHECK-10.12 | `src/projections/` is the ONLY query source for external API consumers | HIGH |
| CHECK-10.13 | No mixed responsibilities in either projection layer | HIGH |

### SDIM-11: Phase 1 Vertical Slice Structure

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | `sandbox` domain exists under canonical domain topology | CRITICAL |
| CHECK-11.2 | `todo` domain exists under canonical domain topology | CRITICAL |
| CHECK-11.3 | Platform layer connects to systems layer only | CRITICAL |
| CHECK-11.4 | Systems layer connects to runtime only | CRITICAL |
| CHECK-11.5 | Runtime connects to engines only | CRITICAL |
| CHECK-11.6 | Engines connect to domain only | CRITICAL |

### SDIM-12: Execution Path Integrity

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-12.1 | No shortcut execution paths exist across layers | CRITICAL |
| CHECK-12.2 | No direct engine invocation from platform or systems | CRITICAL |
| CHECK-12.3 | No domain access from systems or platform | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: structural
status: PASS | FAIL
score: {0-100}
scope: "Full repository structure"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: SDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "structural"
approval: GRANTED | BLOCKED
blocking_violations: {count of CRITICAL/HIGH}
```

---

## PROJECTION STRUCTURAL RULES (S1.5)

| Rule | Description | Severity |
|------|-------------|----------|
| S-PROJ-1 | `src/projections/` is a first-class canonical layer — must exist and contain domain-aligned projection modules | CRITICAL |
| S-PROJ-2 | `src/runtime/projection/` must not leak into domain/systems/platform — no references from those layers | CRITICAL |
| S-PROJ-3 | Projection handlers must only consume events — no command dispatch, no aggregate mutation | CRITICAL |

---

## SCORING

| Start Score | 100 |
|-------------|-----|
| CRITICAL violation | -10 per occurrence |
| HIGH violation | -5 per occurrence |
| MEDIUM violation | -2 per occurrence |
| LOW violation | -1 per occurrence |
| Floor | 0 |
| Pass threshold | >= 80 |
