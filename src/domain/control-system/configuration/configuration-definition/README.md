# control-system / configuration / configuration-definition

**Classification:** control-system  
**Context:** configuration  
**Domain:** configuration-definition

## Purpose
The canonical definition of a configuration key: its name, expected type, default value, and documentation. The definition is the schema contract for a configurable aspect of the system.

## Scope
- Definition registration: name, value type (string|integer|boolean|json), default value, description
- Definition deprecation

## Does Not Own
- Current value or state (→ configuration-state)
- Scope applicability (→ configuration-scope)
- Value resolution (→ configuration-resolution)

## Inputs
- name (unique key), value type, default value (optional), description

## Outputs
- `ConfigurationDefinedEvent`
- `ConfigurationDefinitionDeprecatedEvent`

## Invariants
- Definition ID is deterministic: SHA256 of (name + version)
- Names are globally unique
- Published definitions are immutable

## Dependencies
- `core-system/identifier` — definition ID

## Template conformance
Lifecycle-init pattern: **Pattern B (static-factory)**. `ConfigurationDefinitionAggregate.Define(...)` is a `public static` factory method that always returns a freshly-constructed instance. `Version` is invariably `-1` at factory entry; a `Version >= 0` guard is structurally dead code and is deliberately absent. Idempotency is satisfied by construction. Per `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`.
No `entity/` — aggregate has no child entities.
No `service/` — no stateless cross-aggregate coordination required.
No `specification/` — no composable boolean invariants require extraction from aggregate inline logic.
