# ACTIVATION AUDIT — WBSM v3

```
AUDIT ID:       ACTIVATION-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the domain activation lifecycle to ensure every bounded context is at the correct activation level (D0/D1/D2) as declared in the canonical registry. This audit verifies that scaffolded BCs remain scaffolded, activated BCs have minimum DDD artifacts, and operational BCs have full artifact coverage with engine wiring and runtime integration.

This audit MUST detect:

* Activation level mismatches (BC claims D1 but has only D0 artifacts)
* Activation regressions (BC was D1, now missing artifacts)
* Premature activation claims (BC claims D2 without engine wiring)
* Missing scaffolding in D0 BCs
* Incomplete D1 activation (missing aggregate/event/error)
* Incomplete D2 activation (missing full DDD + engine + runtime)

---

## SCOPE

```
src/domain/                    -> all bounded contexts at all activation levels
src/engines/T2E/               -> engine wiring for D2 BCs
src/runtime/                   -> runtime integration for D2 BCs
claude/registry/               -> canonical activation registry
```

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Activation level mismatch with registry, regression from higher level | Blocks deployment |
| HIGH | Missing required artifacts for declared level, premature D2 claim | Must fix before merge |
| MEDIUM | Incomplete artifact set within level, missing specification/service | Fix within sprint |
| LOW | Missing .gitkeep in D0, cosmetic gap | Fix at convenience |

---

## ACTIVATION LEVEL REFERENCE

| Level | Name | Required Artifacts |
|-------|------|-------------------|
| D0 | Scaffolded | Folder structure (CLASSIFICATION/CONTEXT/DOMAIN with 7 DDD subdirectories) + .gitkeep in each folder |
| D1 | Activated | At minimum: 1 aggregate root, 1+ domain events, 1+ domain errors. Entity/VO/service/spec folders may still have .gitkeep |
| D2 | Operational | Full DDD: aggregates, entities, value objects, events, errors, services, specifications. Engine wiring (T2E handler). Runtime integration (command routing, event dispatch) |

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

### ADIM-01: D0 Scaffolded Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | D0 BC has `aggregate/` folder (with .gitkeep or empty) | HIGH |
| CHECK-01.2 | D0 BC has `entity/` folder | HIGH |
| CHECK-01.3 | D0 BC has `value-object/` folder | HIGH |
| CHECK-01.4 | D0 BC has `event/` folder | HIGH |
| CHECK-01.5 | D0 BC has `error/` folder | HIGH |
| CHECK-01.6 | D0 BC has `service/` folder | HIGH |
| CHECK-01.7 | D0 BC has `specification/` folder | HIGH |
| CHECK-01.8 | D0 BC contains NO implementation files beyond .gitkeep | MEDIUM |

### ADIM-02: D1 Activated Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | D1 BC has at least one aggregate root class in `aggregate/` | CRITICAL |
| CHECK-02.2 | D1 BC has at least one domain event class in `event/` | CRITICAL |
| CHECK-02.3 | D1 BC has at least one domain error class in `error/` | HIGH |
| CHECK-02.4 | D1 aggregate root has a constructor with invariant validation | HIGH |
| CHECK-02.5 | D1 domain events use past-tense naming | HIGH |
| CHECK-02.6 | D1 domain events are immutable (no public setters) | HIGH |

### ADIM-03: D2 Operational Compliance

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | D2 BC has aggregate root(s) with full behavior (methods, state transitions) | CRITICAL |
| CHECK-03.2 | D2 BC has at least one entity in `entity/` | HIGH |
| CHECK-03.3 | D2 BC has at least one value object in `value-object/` | HIGH |
| CHECK-03.4 | D2 BC has comprehensive domain events for all state transitions | HIGH |
| CHECK-03.5 | D2 BC has domain errors for all failure modes | HIGH |
| CHECK-03.6 | D2 BC has at least one domain service in `service/` | HIGH |
| CHECK-03.7 | D2 BC has at least one specification in `specification/` | HIGH |
| CHECK-03.8 | D2 BC has a corresponding T2E command handler in `src/engines/T2E/` | CRITICAL |
| CHECK-03.9 | D2 BC has runtime command routing configured | CRITICAL |
| CHECK-03.10 | D2 BC has runtime event dispatch configured | HIGH |

### ADIM-04: Registry Match Verification

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Every BC in repository exists in canonical activation registry | CRITICAL |
| CHECK-04.2 | Every BC in registry exists in repository | CRITICAL |
| CHECK-04.3 | Registry-declared activation level matches actual artifact state | CRITICAL |
| CHECK-04.4 | No unregistered BCs exist in the domain folder | HIGH |
| CHECK-04.5 | Registry is up-to-date (last modified within acceptable window) | MEDIUM |

### ADIM-05: Activation Regression Detection

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | No BC has fewer artifacts than its declared activation level requires | CRITICAL |
| CHECK-05.2 | No D1 BC has lost its aggregate root since last audit | CRITICAL |
| CHECK-05.3 | No D1 BC has lost its domain events since last audit | CRITICAL |
| CHECK-05.4 | No D2 BC has lost engine wiring since last audit | CRITICAL |
| CHECK-05.5 | No D2 BC has lost runtime integration since last audit | CRITICAL |

### ADIM-06: Activation Progression Validation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | D0 -> D1 transition includes: aggregate + event + error at minimum | HIGH |
| CHECK-06.2 | D1 -> D2 transition includes: full DDD artifacts + engine wiring + runtime integration | HIGH |
| CHECK-06.3 | No BC skips activation levels (D0 -> D2 without D1 artifacts) | HIGH |
| CHECK-06.4 | Activation transitions are documented in change reports | MEDIUM |

### ADIM-07: Cross-Layer Activation Consistency

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | D2 BCs have corresponding shared-layer contracts (repository interfaces) | HIGH |
| CHECK-07.2 | D2 BCs have corresponding infrastructure adapters (repository implementations) | HIGH |
| CHECK-07.3 | D2 BCs have corresponding projection handlers (if read models required) | MEDIUM |
| CHECK-07.4 | D0/D1 BCs do NOT have engine wiring (premature integration) | HIGH |

### ADIM-08: Projection Readiness (D2 Only)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | D2 BC has corresponding domain projection (if queryable) in `src/projections/` | HIGH |
| CHECK-08.2 | Projection consumes domain events only | CRITICAL |
| CHECK-08.3 | Projection does not dispatch commands | CRITICAL |
| CHECK-08.4 | Projection is eventually consistent (no sync dependency on aggregate) | HIGH |

### ADIM-09: E2E Execution Integrity (Phase 1 — E2EDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | D2 BCs demonstrate Platform → Systems → Runtime → Engines → Domain path | CRITICAL |
| CHECK-09.2 | No D2 BC has direct Platform → Engine calls bypassing systems/runtime | CRITICAL |
| CHECK-09.3 | D2 BCs dispatch through runtime only via approved contracts | CRITICAL |
| CHECK-09.4 | D2 BC end-to-end execution produces domain events, persistence, and projections | CRITICAL |
| CHECK-09.5 | D0/D1 BCs do NOT have premature E2E wiring | HIGH |

### ADIM-10: Lifecycle + Workflow Validation (Phase 1 — LWFDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | At least one D2 BC implements lifecycle process (created → active → completed) | CRITICAL |
| CHECK-10.2 | D2 lifecycle transitions enforce aggregate invariants | CRITICAL |
| CHECK-10.3 | At least one D2 BC has a workflow execution path through WSS | CRITICAL |
| CHECK-10.4 | Workflow uses WSS (midstream) orchestration pattern | CRITICAL |
| CHECK-10.5 | Workflow execution produces observable events and projections | HIGH |

### ADIM-11: Sandbox/Todo Mandatory (Phase 1 — SBDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-11.1 | Sandbox bounded context exists in activation registry | CRITICAL |
| CHECK-11.2 | Todo bounded context exists in activation registry | CRITICAL |
| CHECK-11.3 | Both are at minimum D1 activation with executable artifacts | CRITICAL |
| CHECK-11.4 | Both demonstrate full vertical slice per their activation level | CRITICAL |

---

## OUTPUT FORMAT

```yaml
audit: activation
status: PASS | FAIL
score: {0-100}
scope: "Domain activation lifecycle compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: ADIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "activation"
activation_summary:
  total_bcs: {count}
  d0_scaffolded: {count}
  d1_activated: {count}
  d2_operational: {count}
  regressions_detected: {count}
  mismatches_detected: {count}
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
