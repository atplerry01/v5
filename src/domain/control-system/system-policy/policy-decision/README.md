# control-system / system-policy / policy-decision

**Classification:** control-system  
**Context:** system-policy  
**Domain:** policy-decision

## Purpose
Captures the evaluated output of a policy against a subject, action, and resource. Decisions are immutable records — they are never mutated after emission.

## Scope
- Decision outcome: permit / deny / not-applicable / indeterminate
- Obligation set attached to a permit decision
- Decision correlation back to the triggering policy definition

## Does Not Own
- Policy conditions or scope (→ policy-definition)
- Enforcement execution (→ policy-enforcement)

## Inputs
- Evaluation request: subject, action, resource, context
- Resolved policy definitions

## Outputs
- `PolicyDecisionEvent` (permit | deny | not-applicable)

## Invariants
- Every decision traces to exactly one policy definition version
- Decision identity is deterministic: SHA256 of (policyId + subject + action + resource + timestamp)
- Decisions are append-only; no updates

## Dependencies
- `core-system/identifier` — deterministic decision ID
- `core-system/temporal` — decision timestamp

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `PolicyDecisionAggregate.Record(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
