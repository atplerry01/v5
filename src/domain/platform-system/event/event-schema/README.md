# platform-system / event / event-schema

**Classification:** platform-system  
**Context:** event  
**Domain:** event-schema

## Purpose
Canonical schema for an event payload: field declarations, types, required fields, and version. The schema is the contract between event producers and consumers.

## Scope
- Schema registration with field declarations
- Schema versioning and compatibility declaration (backward / forward / full)

## Does Not Own
- Event type registration (→ event-definition)
- Schema validation execution (→ engine layer)

## Inputs
- Schema name, version, field declarations, compatibility mode

## Outputs
- `EventSchemaRegisteredEvent`
- `EventSchemaDeprecatedEvent`

## Invariants
- Schema ID is deterministic: SHA256 of (name + version)
- Published schemas are immutable; breaking changes require a new version
- Compatibility mode must be declared at registration

## Dependencies
- `core-system/identifier` — schema ID
- `platform-system/schema` — delegates to schema/schema-definition for the structural contract
