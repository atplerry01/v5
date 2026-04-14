# Constitutional Audit (Canonical)

**Validates:** [`claude/guards/constitutional.guard.md`](../../guards/constitutional.guard.md)
**Type:** Pure validation layer — defines NO rules. All rules live in the guard.

---

## Purpose

Verify that every operation in this repository is bound to WHYCEPOLICY at the pre-execution gate, that no decision proceeds without policy authorization, and that policy decisions are captured, hashed, and chain-anchored as defined in the constitutional guard.

## Scope

- All runtime entry points, command dispatchers, query handlers, projection consumers
- Composition root (`src/platform/host/`)
- Policy registry, policy declarations, policy bindings
- Decision capture / chain anchoring sinks
- Audit trail outputs

## Source guard

This audit checks the rules defined in [`claude/guards/constitutional.guard.md`](../../guards/constitutional.guard.md). Rule IDs (R1–R34, grouped A–G) referenced below are owned by that guard.

---

## Validation Checklist

### Group A — Policy Binding (Pre-Execution Gate)
- [ ] **R1–R10** — Every operation passes through the WHYCEPOLICY pre-execution binding gate; no bypass paths exist; deny-by-default is the default verdict; chain link is established at the binding gate (PB-09).

### Group B — Policy Declaration & Authorization
- [ ] **R11–R14** — Every policy is declared in the registry; declarations include classification, context, domain; authorization scopes are explicit and enumerated; no implicit or ambient authority.

### Group C — NoPolicyFlag / Violation Capture
- [ ] **R15–R16** — `NoPolicyFlag` is treated as an anomaly and captured to the audit trail; absence of policy binding produces a structured violation, not silent passage.

### Group D — Policy Lifecycle, Composition & Registry
- [ ] **R17–R22** — Policy composition occurs at the composition root only; registry entries match declarations; lifecycle transitions (load/activate/revoke) are deterministic and auditable.

### Group E — Decision Capture, Hashing & Chain Anchoring
- [ ] **R23–R25** — Every policy decision is captured with input hash, output hash, decision verdict, and chain anchor; SHA256 is used; chain anchors are deterministic.

### Group F — Policy Eventification (Integrated New Rules)
- [ ] **R26–R29** — Policy decisions emit events (`PolicyDecisionEvent` per naming convention $10); no silent decisions; eventification is required at the decision boundary.

### Group G — WBSM v3 Global Enforcement (shared)
- [ ] **R30–R34** — GE-01 deterministic execution; GE-02 WHYCEPOLICY enforcement; GE-03 WHYCECHAIN anchoring; GE-04 event-first architecture; GE-05 CQRS enforcement.

---

## Check Procedure

1. Load the constitutional guard rule set (rule IDs R1–R34).
2. For each rule group, execute the validation procedure declared in the guard (`Check Procedure` blocks).
3. Record a verdict per rule: `PASS` / `FAIL` / `N/A` with a one-line evidence pointer (file:line, query, or artifact).
4. Aggregate group verdicts.

## Pass / Fail Criteria

- **PASS:** All R1–R34 verdicts are `PASS` or `N/A` with documented justification.
- **FAIL:** Any rule with severity S0 or S1 in the guard returns `FAIL`.
- **CONDITIONAL:** S2/S3 failures permit conditional pass with remediation captured to `claude/new-rules/` per CLAUDE.md $1c.

## Output Format

```
AUDIT:           constitutional
GUARD:           claude/guards/constitutional.guard.md
EXECUTED:        <ISO-8601>
RULES_CHECKED:   34
PASS:            <count>
FAIL:            <count>
N/A:             <count>
S0_FAILURES:     <list of rule IDs>
S1_FAILURES:     <list of rule IDs>
EVIDENCE:        <file path to detailed verdict log>
VERDICT:         PASS | FAIL | CONDITIONAL
```

## Failure Action

Per CLAUDE.md $12: STOP, no partial completion, structured failure report (STATUS / STAGE / REASON / ACTION_REQUIRED). Per $1c: capture any new rule discovered during this audit to `claude/new-rules/{YYYYMMDD-HHMMSS}-constitutional.md`.
