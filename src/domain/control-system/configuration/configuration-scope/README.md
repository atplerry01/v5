# control-system / configuration / configuration-scope

**Classification:** control-system  
**Context:** configuration  
**Domain:** configuration-scope

## Purpose
Declares the applicability scope of a configuration definition: which classification and context pair the configuration applies to. Enables the same key to have different values per scope.

## Scope
- Scope declaration: definition ID (reference), classification, context (optional — if absent, applies to entire classification)
- Scope priority: context-level overrides classification-level

## Does Not Own
- Value storage (→ configuration-state)
- Value resolution (→ configuration-resolution)

## Inputs
- definition ID, classification, context (optional)

## Outputs
- `ConfigurationScopeDeclaredEvent`
- `ConfigurationScopeRemovedEvent`

## Invariants
- Scope ID is deterministic: SHA256 of (definitionId + classification + context)
- A scope is unique per (definitionId, classification, context) tuple
- Removing a scope does not remove the definition

## Dependencies
- `core-system/identifier` — scope ID and definition reference

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `ConfigurationScopeAggregate.Declare(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
