# configuration-assignment

**Classification:** control-system
**Context:** configuration
**Domain:** configuration-assignment

## Purpose

Links a configuration definition to a specific scope with a concrete value. An assignment is the act of applying a defined configuration key to a declared scope — scoping what value is in effect for a given classification, context, or domain boundary. Assignments are revocable; revocation removes the value from the effective configuration for that scope.

## Aggregate: ConfigurationAssignmentAggregate

| Property | Type | Description |
|---|---|---|
| Id | ConfigurationAssignmentId | Deterministic 64-hex SHA256 identifier |
| DefinitionId | string | Reference to the configuration-definition |
| ScopeId | string | Reference to the configuration-scope |
| Value | string | The assigned configuration value |
| Status | AssignmentStatus | Active / Revoked |

## Invariants

- DefinitionId, ScopeId, and Value must not be empty.
- A revoked assignment cannot be revoked again.

## Events

| Event | Trigger |
|---|---|
| ConfigurationAssignedEvent | Value assigned to a definition-scope pair |
| ConfigurationAssignmentRevokedEvent | Assignment removed from effective configuration |
