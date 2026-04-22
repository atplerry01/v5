# platform-system / command / command-definition

**Classification:** platform-system  
**Context:** command  
**Domain:** command-definition

## Purpose
Registered command type definition: the canonical name, version, schema reference, and ownership (which DomainRoute handles this command type).

## Scope
- Command type registration
- Version declaration and deprecation

## Does Not Own
- Envelope construction (→ command-envelope)
- Routing rules (→ command-routing)

## Inputs
- Command type name, version, schema ID, owner DomainRoute

## Outputs
- `CommandDefinedEvent`
- `CommandDeprecatedEvent`

## Invariants
- Command type ID is deterministic: SHA256 of (typeName + version)
- Type names are globally unique within the platform
- Published definitions are immutable

## Dependencies
- `core-system/identifier` — definition ID
- `platform-system/schema` — schema reference by ID only
