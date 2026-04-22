# access-policy

**Classification:** control-system
**Context:** access-control
**Domain:** access-policy

## Purpose

Governs which roles may perform which actions within a declared scope. An access policy is the control-system's fine-grained authority rule: it binds a scope (resource + action boundary) to a set of allowed roles, and moves through Draft → Active → Retired lifecycle.

## Aggregate: AccessPolicyAggregate

| Property | Type | Description |
|---|---|---|
| Id | AccessPolicyId | Deterministic 64-hex SHA256 identifier |
| Name | string | Human-readable policy name |
| Scope | string | Resource and action boundary this policy governs |
| AllowedRoleIds | IReadOnlySet&lt;string&gt; | Roles permitted by this policy |
| Status | AccessPolicyStatus | Draft / Active / Retired |

## Invariants

- Name and Scope must not be empty.
- Activation requires Draft status.
- Retired policies cannot be reactivated.

## Events

| Event | Trigger |
|---|---|
| AccessPolicyDefinedEvent | Policy authored with scope and allowed roles |
| AccessPolicyActivatedEvent | Policy enters enforcement |
| AccessPolicyRetiredEvent | Policy withdrawn from enforcement |
