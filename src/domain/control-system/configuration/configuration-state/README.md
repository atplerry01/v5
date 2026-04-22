# control-system / configuration / configuration-state

**Classification:** control-system  
**Context:** configuration  
**Domain:** configuration-state

## Purpose
The current value and lifecycle state of a configuration entry at runtime. A state record is versioned and append-only; updates create new state versions.

## Scope
- State record: definition ID (reference), value (opaque string), version, effective period
- State lifecycle: active → superseded → revoked

## Does Not Own
- Configuration key schema (→ configuration-definition)
- Scope constraints (→ configuration-scope)
- Resolution logic (→ configuration-resolution)

## Inputs
- definition ID, value, effective period

## Outputs
- `ConfigurationStateSetEvent`
- `ConfigurationStateRevokedEvent`

## Invariants
- State ID is deterministic: SHA256 of (definitionId + value + effectiveFrom)
- Only one state version is active per definition at any time
- Published state records are immutable; changes create new versions

## Dependencies
- `core-system/identifier` — state ID and definition reference
- `core-system/temporal` — effective period

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `ConfigurationStateAggregate.Set(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
