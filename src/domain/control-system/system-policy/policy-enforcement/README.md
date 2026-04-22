# control-system / system-policy / policy-enforcement

**Classification:** control-system  
**Context:** system-policy  
**Domain:** policy-enforcement

## Purpose
Records the binding of a policy decision to a concrete system effect: what was blocked, allowed, or modified as a result of enforcement.

## Scope
- Enforcement record: decision reference + applied effect + target
- Enforcement outcome: enforced / bypassed / failed
- Obligation fulfillment status attached to permit decisions

## Does Not Own
- Decision logic (→ policy-decision)
- Audit trails (→ audit/audit-log)

## Inputs
- Policy decision record
- Enforcement target (command ID, event ID, or resource reference)

## Outputs
- `PolicyEnforcedEvent`
- `PolicyBypassedEvent` (anomaly — NoPolicyFlag path)

## Invariants
- Every enforcement record references exactly one decision
- NoPolicyFlag enforcement always emits PolicyBypassedEvent as anomaly
- Enforcement records are append-only

## Dependencies
- `core-system/identifier` — enforcement record ID
- `core-system/temporal` — enforcement timestamp
- `platform-system/command` — command envelope reference (by ID only)

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `PolicyEnforcementAggregate.Enforce(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
