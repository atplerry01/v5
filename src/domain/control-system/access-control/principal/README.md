# principal

**Classification:** control-system
**Context:** access-control
**Domain:** principal

## Purpose

The executing entity (human, service, or system) that performs administrative operations within the control plane. A principal binds to an identity and holds a set of assigned roles, forming the subject of every authorization decision.

## Aggregate: PrincipalAggregate

| Property | Type | Description |
|---|---|---|
| Id | PrincipalId | Deterministic 64-hex SHA256 identifier |
| Name | string | Human-readable principal name |
| Kind | PrincipalKind | Human / Service / System |
| IdentityId | string | Reference to the control-system identity |
| RoleIds | IReadOnlySet&lt;string&gt; | Assigned roles governing this principal's authority |
| Status | PrincipalStatus | Active / Suspended / Deactivated |

## Invariants

- Name must not be empty.
- A role may not be assigned twice.
- Deactivated principals cannot be modified.

## Events

| Event | Trigger |
|---|---|
| PrincipalRegisteredEvent | Principal bound to an identity and registered |
| PrincipalRoleAssignedEvent | Role added to principal's authority set |
| PrincipalDeactivatedEvent | Principal permanently removed from active service |
