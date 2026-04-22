# platform-system / schema / schema-definition

**Classification:** platform-system  
**Context:** schema  
**Domain:** schema-definition

## Purpose
Canonical schema definition: named, versioned structural contract declaring fields, their types, required fields, and compatibility guarantees.

## Aggregate: SchemaDefinitionAggregate

**Identity:** `SchemaDefinitionId` — SHA256 of (name + version)

**State:**
| Field | Type | Description |
|---|---|---|
| `SchemaDefinitionId` | value object (Guid) | Deterministic schema ID |
| `SchemaName` | value object (string) | Stable name across versions |
| `Fields` | collection (FieldDescriptor) | name, fieldType, required, description, defaultValue |
| `Version` | int | Monotonically increasing per schema name |
| `CompatibilityMode` | enum | Backward \| Forward \| Full \| None |
| `Status` | enum | Draft \| Published \| Deprecated |

**FieldDescriptor types:** String \| Int \| Long \| Bool \| Float \| Bytes \| Nested \| Array \| Map

## Lifecycle
`Draft` → `Published` → `Deprecated`
- Draft: editable — fields may be amended before publication
- Published: IMMUTABLE — field mutation forbidden; new version = new aggregate instance
- Deprecated: terminal — no new messages; contracts referencing this schema are flagged

## Outputs
- `SchemaDefinitionDraftedEvent`
- `SchemaDefinitionPublishedEvent`
- `SchemaDefinitionDeprecatedEvent`

## Invariants
- SchemaDefinitionId deterministic: SHA256 of (name + version)
- Field names within a schema are unique
- Published schemas are immutable — field mutation after publication is forbidden
- Deprecated schemas cannot be re-published (compensation: new version)
- CompatibilityMode is explicit — never inferred
- At least one field must be declared
- SchemaName is stable across versions (same name, incremented version number)

## Errors
| Error | Condition |
|---|---|
| `SchemaDefinitionAlreadyPublishedError` | Publish attempted on already-published schema |
| `SchemaDefinitionFieldRequiredError` | No fields declared |
| `SchemaDefinitionImmutableError` | Field mutation attempted on Published schema |
| `SchemaDefinitionDeprecatedError` | Action on deprecated schema |

## Does Not Own
- Schema validation execution (→ engine layer)
- Command or event type assignment (→ command-definition, event-definition)
- Contract enforcement (→ schema/contract)
- Version compatibility verdicts (→ schema/versioning)

## Kafka Topic
`whyce.platform.schema.schema-definition.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for SchemaDefinitionId
