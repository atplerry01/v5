# Policy Guard

## Purpose

Enforce WHYCEPOLICY binding rules. Every domain action that modifies state must be authorized by an explicit policy declaration. Policies are first-class architectural artifacts that govern what actions are permitted, under what conditions, and by whom. Unauthorized domain actions are forbidden.

## Scope

All domain commands, aggregate mutations, engine operations, and runtime pipelines. Applies to policy declaration files, command handlers, and enforcement checkpoints. Evaluated at CI, code review, and governance audit.

## Rules

1. **POLICIES DECLARED EXPLICITLY** — Every domain action that mutates aggregate state must have a corresponding policy declaration. Policies are defined as named artifacts with explicit conditions, authorized actors, and scope. No implicit or convention-based authorization — every mutation path must trace to a declared policy.

2. **NO UNAUTHORIZED DOMAIN ACTIONS** — A command handler must not execute a domain mutation unless the command's associated policy has been evaluated and satisfied. If no policy is bound to a command, the command must be rejected at the runtime pipeline before reaching the engine. Default behavior is deny.

3. **POLICY ENFORCEMENT THROUGH RUNTIME** — Policy evaluation occurs in the runtime middleware pipeline, before command dispatch to engines. Runtime checks policy satisfaction as a pipeline step. Engines must not evaluate policies — they assume the command has already been authorized by the time it arrives.

4. **POLICY VIOLATIONS PRODUCE DOMAIN EVENTS** — When a policy check fails (unauthorized action attempted), the system must produce a domain event recording the violation: `PolicyViolationDetectedEvent`. This event includes the attempted action, the policy that was violated, the actor, and the timestamp. Violations are never silently swallowed.

5. **POLICIES ARE AUDITABLE** — All policy evaluations (pass and fail) must be recorded in an audit log. The audit trail includes: policy name, action attempted, actor identity, evaluation result (pass/fail), timestamp, and correlation ID. This enables forensic analysis and compliance reporting.

6. **POLICY COMPOSITION** — Policies can be composed using AND/OR/NOT operators. A compound policy is satisfied only when its sub-policies evaluate correctly. Policy composition must be declarative (not imperative if/else chains). Composite policies are first-class artifacts.

7. **POLICY SCOPE BINDING** — Each policy must declare its scope: which bounded context(s), aggregate(s), and command(s) it applies to. A policy without scope binding is invalid. Scope can be broad (all commands in a BC) or narrow (single command on single aggregate).

8. **POLICY VERSIONING** — Policies must be versioned. When a policy changes, the previous version is retained for audit history. Active policy version is explicitly declared. No silent policy mutation — changes produce `PolicyUpdatedEvent`.

9. **SEPARATION OF POLICY AND DOMAIN RULES** — Policies govern authorization and permission (who can do what, under what conditions). Domain specifications govern business invariants (what is valid state). These are distinct concerns. A domain specification must not check authorization; a policy must not enforce business invariants.

10. **POLICY REGISTRY** — All active policies must be registered in a policy registry (file or configuration). The registry serves as the canonical list of all policies in the system. Unregistered policies are not enforceable. The registry is the source of truth for governance audits.

11. **TEMPORAL POLICIES** — Policies may have effective dates (start/end). A temporal policy is only active during its declared window. Expired policies are automatically deactivated. Temporal evaluation uses the injected time provider (not system clock directly).

12. **ESCALATION POLICIES** — Certain high-impact actions require escalation policies with multi-party approval. Escalation policies declare the required approval chain and quorum. No single-actor policy may authorize a high-impact action unless explicitly permitted by governance configuration.

13. **POLICY DECISION HASHING** — Every policy evaluation must produce a `DecisionHash` — a cryptographic hash of the policy decision including: policy ID, version, action, actor, evaluation result, and timestamp. The hash serves as a tamper-proof record of the decision. Decisions without hashes are not auditable and are therefore invalid.

14. **CHAIN ANCHOR REQUIRED FOR DECISION** — Every policy decision must be anchored to the WhyceChain immutable ledger via a `ChainBlock`. The chain block links the `DecisionHash` to the ledger, creating an immutable audit trail. Policy decisions that are not chain-anchored cannot be verified retroactively and are governance violations.

15. **SIMULATION BEFORE ENFORCEMENT** — Policy changes must support simulation mode before enforcement. A new or modified policy must be evaluable in `simulate` mode where it logs what would happen without actually blocking actions. Simulation results are recorded for review. Only after simulation validation should the policy be promoted to `enforce` mode.

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

1. Enumerate all command types across the system.
2. For each command, verify a policy binding exists in the policy registry.
3. Scan runtime middleware pipeline for policy evaluation step — must be present before command dispatch.
4. Verify policy evaluation middleware produces `PolicyViolationDetectedEvent` on failure.
5. Verify audit log captures all policy evaluations (pass and fail).
6. Scan engine code for policy evaluation logic — must be zero (engines assume pre-authorized).
7. Verify all policies in the registry have explicit scope declarations.
8. Verify policy versioning — check for version field and `PolicyUpdatedEvent` on changes.
9. Verify domain specifications do not check authorization/actor identity.
10. Verify policies do not enforce domain business invariants (state validity).
11. Scan for commands that bypass runtime pipeline (direct handler invocation without policy check).
12. Verify temporal policies use injected time provider.

## Pass Criteria

- Every command has a bound policy in the registry.
- Policy evaluation occurs in runtime middleware for all commands.
- Policy violations produce auditable domain events.
- All policy evaluations (pass/fail) are audit-logged.
- No policy evaluation logic in engines.
- All policies have explicit scope declarations.
- Policies are versioned with change events.
- Clear separation between policies (authorization) and specifications (invariants).

## Fail Criteria

- Command without bound policy in registry.
- Missing policy evaluation step in runtime middleware.
- Policy violation silently swallowed (no event, no log).
- Policy evaluation logic in engine code.
- Policy without scope declaration.
- Policy mutation without versioning or change event.
- Policy enforcing domain business invariant.
- Domain specification checking authorization.
- Command bypassing policy evaluation pipeline.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | Command without bound policy | `TransferFundsCommand` has no policy binding |
| **S0 — CRITICAL** | Policy violation silently swallowed | Failed auth check with no event or log |
| **S0 — CRITICAL** | Command bypasses policy pipeline | Direct handler invocation without auth |
| **S1 — HIGH** | Policy evaluation in engine | Engine checking `if (actor.HasRole("admin"))` |
| **S1 — HIGH** | Missing audit trail | Policy evaluations not logged |
| **S1 — HIGH** | Policy without scope | Policy declared but not bound to any command/BC |
| **S2 — MEDIUM** | Unversioned policy | Policy changed without version increment |
| **S2 — MEDIUM** | Specification checking authorization | `OrderValidSpec` checking actor permissions |
| **S2 — MEDIUM** | Policy enforcing invariant | Policy checking `if (balance >= amount)` |
| **S3 — LOW** | Missing temporal bounds | Policy without effective dates where applicable |
| **S3 — LOW** | Unregistered policy | Policy class exists but not in registry |

## Enforcement Action

- **S0**: Block merge. Fail CI. Mandatory remediation before any further review.
- **S1**: Block merge. Fail CI. Must resolve in current PR.
- **S2**: Warn in CI. Must resolve within sprint.
- **S3**: Advisory. Track for governance review.

All violations produce a structured report:
```
POLICY_GUARD_VIOLATION:
  command: <command name>
  policy: <policy name if applicable>
  file: <path>
  rule: <rule number>
  severity: <S0-S3>
  violation: <description>
  expected: <correct policy binding>
  actual: <detected gap>
  remediation: <fix instruction>
```

---

## NEW RULES INTEGRATED — 2026-04-07

- **POL-AUDIT-01**: After every WHYCEPOLICY evaluation (allow OR deny), runtime pipeline MUST emit a PolicyEvaluatedEvent / PolicyDeniedEvent containing DecisionHash, IdentityId, PolicyName, IsAllowed. Decision must be independently auditable as an event, not only via chain DecisionHash.

## NEW RULES INTEGRATED — 2026-04-07 (policy eventification)

- **POLICY-EVENT-REQUIRED-01** (S0): Every WHYCEPOLICY evaluation (allow OR deny) MUST emit a domain event
  (PolicyEvaluatedEvent or PolicyDeniedEvent) carrying DecisionHash, IdentityId, PolicyName, CorrelationId,
  CausationId. A DecisionHash without a corresponding event is a governance violation. Extends POL-AUDIT-01.
- **POLICY-NO-SILENT-DECISION-01** (S0): A policy evaluation that produces a DecisionHash but does not appear
  as a domain event in EventStore is forbidden. Audit trail must be queryable via the event stream, not only
  via chain anchors.
- Source: `claude/new-rules/_archives/20260407-190000-policy-eventification.md`.
