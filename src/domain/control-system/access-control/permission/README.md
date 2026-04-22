# control-system / access-control / permission

**Classification:** control-system  
**Context:** access-control  
**Domain:** permission

## Purpose
Discrete, immutable permission definition: a named capability scoped to a resource type and action set.

## Scope
- Permission definition and versioning
- Permission scope: resource type + action mask (read / write / admin)

## Does Not Own
- Role aggregation (→ role domain)
- Subject assignment (→ authorization domain)

## Inputs
- Permission name, resource scope, action mask

## Outputs
- `PermissionDefinedEvent`
- `PermissionDeprecatedEvent`

## Invariants
- Permission ID is deterministic: SHA256 of (name + resourceScope + actionMask)
- Published permissions are immutable; deprecation requires a successor

## Dependencies
- `core-system/identifier` — permission ID

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `PermissionAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
