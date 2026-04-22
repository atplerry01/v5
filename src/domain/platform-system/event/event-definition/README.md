# platform-system / event / event-definition

**Classification:** platform-system  
**Context:** event  
**Domain:** event-definition

## Purpose
Registered event type definition: canonical name, version, schema reference, and source ownership (which DomainRoute emits this event type).

## Scope
- Event type registration
- Version declaration and deprecation

## Does Not Own
- Envelope construction (→ event-envelope)
- Schema content (→ event-schema)

## Inputs
- Event type name, version, schema ID, source DomainRoute

## Outputs
- `EventDefinedEvent`
- `EventDeprecatedEvent`

## Invariants
- Event type ID is deterministic: SHA256 of (typeName + version)
- Type names are globally unique within the platform
- Published definitions are immutable

## Dependencies
- `core-system/identifier` — definition ID
- `platform-system/schema` — schema reference by ID only
