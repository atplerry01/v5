# identity

**Classification:** control-system
**Context:** access-control
**Domain:** identity

## Purpose

System-level administrative identity within the control-system access-control boundary. Represents service accounts, admin operators, and system agents that act within the control plane. This is **not** end-user identity management — user-facing identity (credentials, federation, verification) belongs to `trust-system/identity`.

## Aggregate: IdentityAggregate

| Property | Type | Description |
|---|---|---|
| Id | IdentityId | Deterministic 64-hex SHA256 identifier |
| Name | string | Human-readable identity name |
| Kind | IdentityKind | ServiceAccount / AdminOperator / SystemAgent |
| Status | IdentityStatus | Active / Suspended / Deactivated |

## Invariants

- Name must not be empty.
- Suspended identities may be deactivated; deactivated identities are permanently inactive.
- Suspension requires a non-empty reason.

## Events

| Event | Trigger |
|---|---|
| IdentityRegisteredEvent | New system-level identity registered |
| IdentitySuspendedEvent | Identity temporarily suspended |
| IdentityDeactivatedEvent | Identity permanently deactivated |
