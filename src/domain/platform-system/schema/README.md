# platform-system / schema

**Classification:** platform-system  
**Context:** schema

## Purpose
Owns the canonical schema definition model shared across all communication contracts (commands and events). A schema is the structural contract for a payload — independent of whether that payload is a command or event.

## Domains
| Domain | Responsibility |
|---|---|
| `schema-definition` | Canonical schema structure: field declarations, types, required set, version, compatibility mode |
| `contract` | Contract binding a schema to a publisher and subscriber constraints |
| `versioning` | Version evolution model: breaking vs non-breaking classification, compatibility verdicts |
| `serialization` | Serialization format specification: encoding, schema mapping, round-trip guarantees |

## Does Not Own
- Command-specific schemas (→ command/command-definition references schema by ID)
- Event-specific schemas (→ event/event-schema references schema by ID)
- Schema validation execution (→ engine layer)
- Business meaning of schema fields (→ consuming domain systems)

## Outputs
- `SchemaDefinitionDraftedEvent`, `SchemaDefinitionPublishedEvent`, `SchemaDefinitionDeprecatedEvent`
- `ContractRegisteredEvent`, `ContractSubscriberAddedEvent`, `ContractDeprecatedEvent`
- `VersioningRuleRegisteredEvent`, `VersioningRuleVerdictIssuedEvent`
- `SerializationFormatRegisteredEvent`, `SerializationFormatDeprecatedEvent`

## Invariants
- Schema IDs deterministic: SHA256 of (name + version)
- Published schemas are IMMUTABLE — field mutation after publication is forbidden
- Compatibility mode is explicit: Backward | Forward | Full | None
- Avro and Protobuf serialization formats MUST reference a SchemaRef
- Breaking changes in a Backward-compatible schema = error at versioning rule registration
- Schemas MUST NOT be inferred at runtime — explicit declaration required

## Dependencies
- `core-system/identifier` — deterministic IDs via IIdGenerator
