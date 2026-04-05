# Policy Binding Guard

> WBSM v3 | CANONICAL | VERSION 1.1

## Purpose

Enforce that every Claude execution has a valid, active, scope-matched policy context bound before any guard checks, prompt execution, or audit runs proceed. This is the first gate in the execution pipeline.

## Scope

All Claude execution contexts — applies universally before any other guard.

## Execution Order

This guard runs FIRST in the pipeline:

```
Policy Binding → Prompt Integrity → Guard Checks → Execution → Audit → Trace
```

---

## Rules

### PB-01: Policy ID Required

Every execution context MUST include a `policyId` field.

- **Check**: `context.policyId` is present and non-empty
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

### PB-02: Policy Must Be Resolved

The `policyId` must resolve to a known policy (built-in or custom).

- **Check**: Policy exists in DEFAULT_POLICIES or custom policies directory
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

### PB-03: Policy Must Be Active

Resolved policy must have `active: true`.

- **Check**: `policy.active === true`
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

### PB-04: Policy Scope Must Match Execution Domain

If the execution targets a specific domain/layer, the policy scope must include it.

- **Check**: Target layer is present in `policy.scope[]`
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

### PB-05: Policy Version Must Be Current

Policy version must match or exceed the minimum required version for WBSM v3.

- **Check**: `policy.version >= "3.0.0"`
- **Severity**: S1-HIGH
- **Fail action**: BLOCK execution, warn operator

### PB-06: Policy Rules Must Be Non-Empty

A valid policy must declare at least one enforcement rule.

- **Check**: `policy.rules.length > 0`
- **Severity**: S1-HIGH
- **Fail action**: BLOCK execution

### PB-07: BLOCK-Level Rules Must Be Present

For structural and behavioral policies, at least one BLOCK-level rule is required.

- **Check**: `policy.rules.some(r => r.enforcement === "BLOCK")`
- **Severity**: S2-MEDIUM
- **Fail action**: WARN, allow execution with advisory

### PB-08: Policy Source Validation

Policy MUST originate from an external authoritative source. Hardcoded or inline policy definitions are FORBIDDEN.

- **Check**: `policySource` is defined and not null; policy is resolved from external file (`registry/policies.json`) or API (OPA / WhycePolicy), never from runtime memory or hardcoded constants
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

**FAIL IF**:
- `policySource === null` or `policySource === undefined`
- Policy resolved from hardcoded `DEFAULT_POLICIES` or inline object literals
- Policy resolved from runtime memory without external backing

### PB-09: POLICY → CHAIN LINK REQUIRED

Every policy decision must be anchored to the WhyceChain immutable ledger. The `DecisionHash` produced by policy evaluation must be recorded as a `ChainBlock` entry before execution proceeds. Decisions without chain anchoring are not governance-compliant and must be rejected.

- **Check**: `chainBlock.decisionHash === policyDecision.hash` and `chainBlock` is persisted
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

### PB-10: POLICY CONTEXT PROPAGATION

The policy decision context must flow through the entire runtime pipeline. Once a policy is evaluated and bound, its decision (including `policyId`, `version`, `decisionHash`, `result`, and `actor`) must be available to all downstream middleware, the engine dispatcher, and the chain anchoring step. Context loss at any pipeline stage is a critical violation.

- **Check**: `pipelineContext.policyDecision` is present and complete at every middleware stage
- **Severity**: S0-CRITICAL
- **Fail action**: BLOCK execution immediately

---

## WBSM v3 GLOBAL ENFORCEMENT

### GE-01: DETERMINISTIC EXECUTION (MANDATORY)

- No `Guid.NewGuid()`, `DateTime.Now`, `DateTime.UtcNow`, or random generators in domain, engine, or runtime code.
- Must use:
  - `IIdGenerator` for identity generation
  - `ITimeProvider` for temporal operations

### GE-02: WHYCEPOLICY ENFORCEMENT

- All state mutations must pass policy validation.
- Guards must verify:
  - Policy exists for the action
  - Policy has been evaluated
  - Policy decision is attached to the execution context

### GE-03: WHYCECHAIN ANCHORING

- All critical actions must produce:
  - `DecisionHash` — cryptographic hash of the policy decision
  - `ChainBlock` — immutable ledger entry anchoring the action

### GE-04: EVENT-FIRST ARCHITECTURE

- All state changes must emit domain events.
- No silent mutations — every aggregate state transition must produce at least one event.

### GE-05: CQRS ENFORCEMENT

- Write model ≠ Read model. They are strictly separated.
- No read-model (projection) usage in write-side decisions.
- Command handlers must never query projection stores.

---

## Check Procedure

1. Verify policy source is defined and external (PB-08)
2. Extract `policyId` from execution context (PB-01)
3. Resolve policy from external source (PB-02)
4. Verify `active` status (PB-03)
5. Verify scope coverage against target domain (PB-04)
6. Verify version meets minimum (PB-05)
7. Verify rules are non-empty (PB-06)
8. Verify BLOCK-level rules exist for structural/behavioral policies (PB-07)

## Pass Criteria

- All S0 and S1 rules pass
- Policy context successfully bound to execution

## Fail Criteria

- ANY S0 rule fails → immediate BLOCK
- ANY S1 rule fails → BLOCK with diagnostic
- S2 rules → WARN only

## Output Format

```yaml
guard: policy-binding
status: PASS | FAIL
policyId: "<resolved policy ID>"
version: "<policy version>"
scope: [<covered scopes>]
ruleCount: <number>
blockRuleCount: <number>
violations:
  - rule: "PB-XX"
    severity: "S0-CRITICAL | S1-HIGH | S2-MEDIUM"
    message: "<description>"
    action: "BLOCK | WARN"
decision: ALLOW | BLOCK
reason: "<human-readable summary>"
```

## CI Phase

- **pre-commit**: Not required
- **pre-push**: Required for all domain/engine/runtime changes
- **pull-request**: Required
- **merge**: Required
- **deploy**: Required

## Registry Entry

Registered in `guard.registry.json` as `policy-binding` with severity `blocking`.
