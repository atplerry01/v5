# core-system / identifier / entity-reference

**Classification:** core-system  
**Context:** identifier  
**Domain:** entity-reference

## Purpose
A typed reference to a specific domain entity: combines a `GlobalIdentifier` with a type discriminator (the entity's classification + context + domain path) so that references are self-describing and resolvable without out-of-band knowledge.

## Scope
- `EntityReference` value object: identifier (GlobalIdentifier), entityType (string — `{classification}/{context}/{domain}`)
- Equality: by identifier value only; entityType is informational

## Does Not Own
- Entity resolution or lookup (→ engine / runtime)
- Domain-specific reference types (each domain wraps EntityReference if needed)

## Inputs
- identifier (GlobalIdentifier), entityType (non-null, format: `{classification}/{context}/{domain}`)

## Outputs
- `EntityReference` value object — no domain events

## Invariants
- identifier must be a valid GlobalIdentifier
- entityType follows the three-segment path format
- Immutable once constructed

## Dependencies
- `core-system/identifier/global-identifier` — identifier type
