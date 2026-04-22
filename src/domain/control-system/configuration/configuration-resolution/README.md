# control-system / configuration / configuration-resolution

**Classification:** control-system  
**Context:** configuration  
**Domain:** configuration-resolution

## Purpose
The resolved value record: what configuration was in effect for a given definition, scope, and point in time. Resolution records are the audit-safe output of the configuration system — they capture what value was actually applied.

## Scope
- Resolution record: definition ID, scope ID, state ID, resolved value, resolved-at timestamp
- Resolution is immutable — it is a historical fact

## Does Not Own
- Resolution logic execution (→ engine layer)
- Value storage (→ configuration-state)
- Scope declarations (→ configuration-scope)

## Inputs
- definition ID, scope ID, state ID, resolved value, resolved-at timestamp

## Outputs
- `ConfigurationResolvedEvent`

## Invariants
- Resolution ID is deterministic: SHA256 of (definitionId + scopeId + stateId + resolvedAt)
- Resolution records are immutable once created
- Every resolution references a valid definition, scope, and state record

## Dependencies
- `core-system/identifier` — resolution ID and all reference IDs
- `core-system/temporal` — resolved-at timestamp

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `ConfigurationResolutionAggregate.Resolve(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
