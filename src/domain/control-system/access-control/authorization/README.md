# control-system / access-control / authorization

**Classification:** control-system  
**Context:** access-control  
**Domain:** authorization

## Purpose
Authorization record binding a subject (actor) to a resolved set of permissions at a point in time. Represents the evaluated access state for a subject.

## Scope
- Authorization record creation from role resolution
- Authorization revocation and expiry

## Does Not Own
- Role or permission definitions (→ role, permission domains)
- Session management (deferred — identity/session lifecycle is excluded from control-system)
- Policy evaluation (→ system-policy/policy-decision)

## Inputs
- Subject ID + assigned role set

## Outputs
- `AuthorizationGrantedEvent`
- `AuthorizationRevokedEvent`

## Invariants
- Authorization ID is deterministic: SHA256 of (subjectId + roleSet + validFrom)
- Revocation is explicit and irreversible for a given record; renewal creates a new record

## Dependencies
- `core-system/identifier` — subject and authorization IDs
- `core-system/temporal` — authorization validity window

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `AuthorizationAggregate.Grant(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
