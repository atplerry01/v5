# control-system / access-control / role

**Classification:** control-system  
**Context:** access-control  
**Domain:** role

## Purpose
Named role that aggregates a set of permissions. Roles are the primary unit of access grant; subjects receive roles, not individual permissions.

## Scope
- Role definition and permission composition
- Role hierarchy (role inheriting another role's permission set)

## Does Not Own
- Subject assignment (→ authorization domain)
- Permission definitions (→ permission domain)

## Inputs
- Role name, permission set, optional parent role

## Outputs
- `RoleDefinedEvent`
- `RolePermissionAddedEvent`
- `RoleDeprecatedEvent`

## Invariants
- Role ID is deterministic: SHA256 of (name + version)
- A role's effective permission set is the union of its own permissions and all inherited permissions
- Circular role inheritance is forbidden

## Dependencies
- `core-system/identifier` — role ID

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `RoleAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
