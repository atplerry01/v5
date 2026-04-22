# control-system / system-policy / policy-definition

**Classification:** control-system  
**Context:** system-policy  
**Domain:** policy-definition

## Purpose
Canonical representation of a system policy: its identity, scope, conditions under which it applies, and the effect it produces.

## Scope
- Policy version lifecycle (draft → published → deprecated)
- Policy scope declarations (classification, context, action)
- Condition expression trees

## Does Not Own
- Policy evaluation logic (→ policy-decision)
- Enforcement records (→ policy-enforcement)
- Access subjects or roles (→ access-control)

## Inputs
- Policy authoring intent from configuration or bootstrap
- Scope descriptor (DomainRoute + action mask)

## Outputs
- `PolicyDefinedEvent`
- `PolicyDeprecatedEvent`

## Invariants
- Policy identity is deterministic: SHA256 of (scope + version)
- Published policies are immutable
- Exactly one active version per (scope, name) at any time

## Dependencies
- `core-system/identifier` — deterministic policy ID generation
- `core-system/temporal` — policy validity windows

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `PolicyDefinitionAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
