# SYSTEMS AUDIT — WBSM v3

```
AUDIT ID:       SYSTEMS-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the systems layer to ensure strict compliance with WBSM v3 composition-only architecture. The systems layer (upstream/midstream/downstream) is responsible exclusively for composition — intent interpretation, workflow definition, and context selection. It must contain ZERO execution logic, ZERO domain mutation, and ZERO direct persistence.

This audit MUST detect:

* Execution logic in systems layer
* Domain mutation from systems
* Direct persistence calls from systems
* Incorrect upstream/midstream/downstream placement
* Engine composition bypassing runtime
* Boundary contract violations

---

## SCOPE

```
src/systems/upstream/    -> external integration composition
src/systems/midstream/   -> workflow/service composition
  ├── wss/               -> Workflow Scheduling System
  ├── heos/              -> HEOS subsystem
  └── whyceatlas/        -> WhyceAtlas subsystem
src/systems/downstream/  -> business intent interpretation
```

Excluded: `src/domain/`, `src/engines/`, `src/runtime/`, `src/platform/`, `infrastructure/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Execution logic in systems, domain mutation, runtime bypass | Blocks deployment |
| HIGH | Direct persistence, incorrect stream placement, boundary violation | Must fix before merge |
| MEDIUM | Non-standard composition pattern, missing contract | Fix within sprint |
| LOW | Missing documentation, naming deviation | Fix at convenience |

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

---

## AUDIT DIMENSIONS

### SYDIM-01: Composition-Only Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | No aggregate method calls (Create, Update, Delete operations) in systems layer | CRITICAL |
| CHECK-01.2 | No command handler implementations in systems layer | CRITICAL |
| CHECK-01.3 | No event handler implementations in systems layer | CRITICAL |
| CHECK-01.4 | No business rule evaluation in systems layer | CRITICAL |
| CHECK-01.5 | Systems layer performs only composition, routing, and delegation | HIGH |

### SYDIM-02: No Domain Mutation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | No `new` keyword for domain aggregates in systems code | CRITICAL |
| CHECK-02.2 | No domain service invocation from systems | CRITICAL |
| CHECK-02.3 | No domain event creation from systems | CRITICAL |
| CHECK-02.4 | No domain value object mutation from systems | HIGH |

### SYDIM-03: Upstream/Midstream/Downstream Placement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | Downstream contains only business intent interpretation and context selection | HIGH |
| CHECK-03.2 | Midstream/WSS contains only workflow definitions and routing | HIGH |
| CHECK-03.3 | Midstream/HEOS contains only its scoped composition | HIGH |
| CHECK-03.4 | Midstream/WhyceAtlas contains only its scoped composition | HIGH |
| CHECK-03.5 | Upstream contains only external-facing composition | HIGH |
| CHECK-03.6 | No cross-stream direct references (downstream does not call upstream) | HIGH |

### SYDIM-04: Engine Composition Through Runtime

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Systems layer does not directly reference engine classes | CRITICAL |
| CHECK-04.2 | Systems layer composes engine invocation through `IRuntimeControlPlane` only | CRITICAL |
| CHECK-04.3 | Downstream interacts with WSS exclusively through `IWorkflowRouter` | CRITICAL |
| CHECK-04.4 | WSS interacts with Runtime exclusively through `IRuntimeControlPlane` | CRITICAL |

### SYDIM-05: No Direct Persistence

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | No `DbContext` or ORM references in systems layer | CRITICAL |
| CHECK-05.2 | No direct SQL queries in systems code | CRITICAL |
| CHECK-05.3 | No repository implementations in systems layer | HIGH |
| CHECK-05.4 | No file system write operations in systems | HIGH |
| CHECK-05.5 | No direct cache write operations in systems | HIGH |

### SYDIM-06: WSS Declarative Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | WSS workflow definitions are declarative (configuration/metadata, not imperative logic) | HIGH |
| CHECK-06.2 | WSS does not contain `if/else/switch` business decision logic | CRITICAL |
| CHECK-06.3 | WSS routing is based on workflow metadata, not runtime state inspection | HIGH |
| CHECK-06.4 | WSS does not access domain state directly | CRITICAL |

### SYDIM-07: Boundary Contract Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Downstream-to-WSS boundary uses `IWorkflowRouter` contract only | CRITICAL |
| CHECK-07.2 | WSS-to-Runtime boundary uses `IRuntimeControlPlane` contract only | CRITICAL |
| CHECK-07.3 | No internal class imports across system boundaries | HIGH |
| CHECK-07.4 | Boundary contracts defined in shared layer | HIGH |

### SYDIM-08: Projection Usage

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | Systems layer queries only via projections (not aggregates) | HIGH |
| CHECK-08.2 | Systems layer does not mutate projection data | CRITICAL |

### SYDIM-09: E2E Execution Integrity (Phase 1 — E2EDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Platform → Systems → Runtime → Engines → Domain path exists and systems participates correctly | CRITICAL |
| CHECK-09.2 | No direct Platform → Runtime or Platform → Engine calls bypass systems layer | CRITICAL |
| CHECK-09.3 | Systems dispatch to runtime only via approved contracts (`IRuntimeControlPlane`) | CRITICAL |
| CHECK-09.4 | Runtime is the ONLY execution entry point — systems never invokes engines directly | CRITICAL |
| CHECK-09.5 | End-to-end execution through systems produces domain events, persistence, and projections | CRITICAL |

### SYDIM-10: Lifecycle + Workflow Validation (Phase 1 — LWFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | At least one lifecycle process routable through systems composition | CRITICAL |
| CHECK-10.2 | Lifecycle transitions composed declaratively in systems (not imperatively) | CRITICAL |
| CHECK-10.3 | At least one workflow execution path exists through WSS (midstream) | CRITICAL |
| CHECK-10.4 | Workflow uses WSS orchestration pattern — no ad-hoc execution paths | CRITICAL |
| CHECK-10.5 | Workflow execution through systems produces observable events and projections | HIGH |

### SYDIM-11: Execution Path Enforcement (Phase 1 Alignment)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | Downstream routes intent only (no execution) | CRITICAL |
| CHECK-11.2 | WSS defines workflow but does not execute logic | CRITICAL |
| CHECK-11.3 | Systems invoke runtime via IRuntimeControlPlane ONLY | CRITICAL — *reinforces CHECK-04.2, CHECK-09.3* |
| CHECK-11.4 | No bypass from systems to engines | CRITICAL — *reinforces CHECK-04.1, CHECK-09.4* |

---

## OUTPUT FORMAT

```yaml
audit: systems
status: PASS | FAIL
score: {0-100}
scope: "Systems layer compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: SYDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "systems"
approval: GRANTED | BLOCKED
blocking_violations: {count of CRITICAL/HIGH}
```

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
