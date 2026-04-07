# POLICY AUDIT — WBSM v3

```
AUDIT ID:       POLICY-AUDIT-v2
REVISION:       REV 3
DATE:           2026-04-04
AUTHOR:         Architecture Office
STATUS:         ACTIVE
```

## PURPOSE

Audit the policy layer to ensure strict compliance with WBSM v3 policy governance rules. All system actions must be policy-gated through explicit WHYCEPOLICY bindings, enforced at runtime via T0U engine evaluation, and produce auditable policy events. No unauthorized actions may bypass policy enforcement.

This audit MUST detect:

* Missing policy declarations for protected actions
* WHYCEPOLICY binding gaps
* Policy enforcement bypasses
* Missing policy event generation
* Non-auditable policy decisions
* Unauthorized action execution paths

---

## SCOPE

```
src/engines/T0U/       -> policy evaluation engine
src/runtime/           -> policy enforcement orchestration
src/domain/            -> policy domain models (if applicable)
src/shared/            -> policy contracts and interfaces
infrastructure/        -> policy persistence adapters
```

Excluded: `src/engines/T1M-T4A/` (consumers of policy, not definers), `bin/`, `obj/`

---

## SEVERITY CLASSIFICATION

| Severity | Description | Impact |
|----------|-------------|--------|
| CRITICAL | Missing policy gate on protected action, policy bypass path | Blocks deployment |
| HIGH | Missing WHYCEPOLICY binding, non-auditable decision, missing event | Must fix before merge |
| MEDIUM | Incomplete policy declaration, missing policy metadata | Fix within sprint |
| LOW | Missing policy documentation, non-standard naming | Fix at convenience |

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

### PDIM-01: Explicit Policy Declarations

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-01.1 | Every protected domain action has a corresponding policy declaration | CRITICAL |
| CHECK-01.2 | Policy declarations specify required conditions, permissions, and constraints | HIGH |
| CHECK-01.3 | Policy declarations are versioned (policy changes are trackable) | HIGH |
| CHECK-01.4 | No implicit or undeclared policies (all policies are explicit artifacts) | CRITICAL |
| CHECK-01.5 | Policy scope clearly defines which actions/resources are governed | HIGH |

### PDIM-02: WHYCEPOLICY Binding

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-02.1 | Each policy declaration has a WHYCEPOLICY binding attribute/annotation | CRITICAL |
| CHECK-02.2 | WHYCEPOLICY bindings reference valid policy IDs | HIGH |
| CHECK-02.3 | WHYCEPOLICY bindings are applied at command handler level | HIGH |
| CHECK-02.4 | No command handlers for protected actions exist without WHYCEPOLICY binding | CRITICAL |
| CHECK-02.5 | WHYCEPOLICY bindings are immutable once deployed (versioned, not edited in place) | HIGH |

### PDIM-03: Runtime Enforcement

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-03.1 | T0U engine evaluates policy BEFORE T1M/T2E execution | CRITICAL |
| CHECK-03.2 | Policy evaluation result is checked by runtime before dispatching to execution engines | CRITICAL |
| CHECK-03.3 | Policy denial prevents command execution (hard gate, not advisory) | CRITICAL |
| CHECK-03.4 | Policy evaluation timeout does not default to allow (fail-closed) | HIGH |
| CHECK-03.5 | Policy context (actor, resource, action, environment) passed to T0U evaluator | HIGH |

### PDIM-04: Policy Event Generation

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-04.1 | Every policy evaluation produces a policy event (approved or denied) | HIGH |
| CHECK-04.2 | Policy events include: policy ID, actor, action, result, timestamp, reason | HIGH |
| CHECK-04.3 | Policy denial events include denial reason and violated constraint | HIGH |
| CHECK-04.4 | Policy events are persisted (not fire-and-forget) | HIGH |
| CHECK-04.5 | Policy events follow past-tense naming (`PolicyEvaluatedEvent`, `PolicyDeniedEvent`) | MEDIUM |

### PDIM-05: Auditability

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-05.1 | All policy decisions are traceable via correlation ID | HIGH |
| CHECK-05.2 | Policy audit trail is immutable (append-only) | CRITICAL |
| CHECK-05.3 | Policy audit records include full evaluation context | HIGH |
| CHECK-05.4 | Policy audit records are queryable for compliance reporting | MEDIUM |
| CHECK-05.5 | Historical policy versions are preserved for retroactive audit | HIGH |

### PDIM-06: No Unauthorized Actions

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-06.1 | No command execution path exists that bypasses T0U policy evaluation | CRITICAL |
| CHECK-06.2 | No admin/superuser override that skips policy (unless itself policy-gated) | CRITICAL |
| CHECK-06.3 | Background jobs and scheduled tasks are also policy-evaluated | HIGH |
| CHECK-06.4 | Internal system actions have system-level policy bindings | HIGH |
| CHECK-06.5 | No hardcoded permission checks that circumvent policy engine | HIGH |

### PDIM-07: Projection Policy Scope

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-07.1 | Projections do not require policy evaluation (read-only) | HIGH |
| CHECK-07.2 | Projection queries do not bypass data access rules (if applicable) | MEDIUM |

### PDIM-08: Policy & Guard Enforcement (Phase 1 — PGDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-08.1 | Policy middleware exists in runtime pipeline and is registered | CRITICAL |
| CHECK-08.2 | Policy evaluation occurs before any execution dispatch | CRITICAL |
| CHECK-08.3 | Guard middleware exists (pre-execution + post-execution validation) | CRITICAL |
| CHECK-08.4 | No execution path bypasses the policy layer | CRITICAL |
| CHECK-08.5 | Policy decision is traceable via DecisionHash | HIGH |

### PDIM-09: E2E Execution Integrity (Phase 1 — E2EDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-09.1 | Policy evaluation is part of the Platform → Systems → Runtime → Engines → Domain path | CRITICAL |
| CHECK-09.2 | No direct Platform → Engine calls bypass policy evaluation | CRITICAL |
| CHECK-09.3 | Policy enforcement occurs within runtime before engine dispatch | CRITICAL |
| CHECK-09.4 | End-to-end execution includes policy decision in audit trail | CRITICAL |
| CHECK-09.5 | Policy evaluation produces traceable events in execution pipeline | HIGH |

### PDIM-10: Determinism Enforcement (Phase 1 — DETDIM-01)

| Check | Description | Severity |
|-------|-------------|----------|
| CHECK-10.1 | No `DateTime.UtcNow` or non-deterministic time usage in policy evaluation | CRITICAL |
| CHECK-10.2 | No `Guid.NewGuid()` outside deterministic helper in policy code | CRITICAL |
| CHECK-10.3 | Policy decision IDs generated via `DeterministicIdHelper` only | CRITICAL |
| CHECK-10.4 | Policy evaluation replay produces identical decisions | HIGH |
| CHECK-10.5 | DecisionHash is deterministically computed (SHA256) | HIGH |

---

## OUTPUT FORMAT

```yaml
audit: policy
status: PASS | FAIL
score: {0-100}
scope: "Policy governance compliance"
timestamp: {ISO-8601}
violations:
  - check: CHECK-XX.X
    dimension: PDIM-XX
    severity: CRITICAL | HIGH | MEDIUM | LOW
    description: "{what was found}"
    impacted_files:
      - "{file path}"
    remediation: "{how to fix}"
    drift_classification: "policy"
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

---

## NEW CHECKS INTEGRATED — 2026-04-07

- **CHECK-POL-AUDIT-01**: Verify runtime emits PolicyEvaluatedEvent (or PolicyDeniedEvent) on every policy evaluation path (allow + deny). Event payload must include DecisionHash, IdentityId, PolicyName, IsAllowed.

### CHECK: POLICY-CHAIN-01
Verify every policy decision is anchored to WhyceChain.

## TRACEABILITY REFERENCE — 2026-04-07

MAP: see claude/traceability/guard-traceability.map.md
- Each CHECK in this audit maps to a Guard Rule ID, Enforcement Point, and Evidence as defined in the master traceability map.
