# platform-system / envelope / payload

**Classification:** platform-system  
**Context:** envelope  
**Domain:** payload

## Purpose
Owns the opaque payload contract — defines how payload content is typed, encoded,
and schema-referenced within a message envelope. The payload bytes themselves are
opaque to this domain; this domain owns the CONTRACT for describing a payload.

## Primary Usage: Value Object (embedded in message-envelope)

`PayloadValueObject` is the primary artifact consumed by `message-envelope`:

| Field | Type | Description |
|---|---|---|
| `TypeRef` | string | Fully qualified payload type e.g. `platform.command.RegisterCommandDefinition.v1` |
| `Encoding` | enum | Json \| Avro \| Protobuf \| Binary |
| `SchemaRef` | string | Reference to platform-system/schema/schema-definition ID |
| `Bytes` | ReadOnlyMemory\<byte\> | Opaque encoded payload content |

## Aggregate: PayloadSchemaAggregate (schema contract registration)

Used to register canonical payload schema contracts for type governance.

**Identity:** `PayloadSchemaId` — SHA256 of (typeRef + encoding + schemaRef + version)

**State:**
| Field | Type | Description |
|---|---|---|
| `PayloadSchemaId` | value object (Guid) | Deterministic schema contract ID |
| `TypeRef` | value object (string) | Canonical payload type reference |
| `Encoding` | enum value object | Json \| Avro \| Protobuf \| Binary |
| `SchemaRef` | value object (string) | References platform-system/schema/schema-definition |
| `Version` | int | Schema contract version |
| `MaxSizeBytes` | value object (long, nullable) | Optional maximum payload size |
| `Status` | enum | Active \| Deprecated |

## Events
| Event | Trigger |
|---|---|
| `PayloadSchemaRegisteredEvent` | New payload schema contract registered |
| `PayloadSchemaDeprecatedEvent` | Payload schema contract deprecated |

## Invariants
- PayloadSchemaId deterministic from (typeRef + encoding + schemaRef + version)
- TypeRef must be non-empty
- Encoding must be explicit — never inferred at runtime (per schema invariants)
- Avro and Protobuf encodings MUST reference a SchemaRef
- Json encoding SHOULD reference a SchemaRef (recommended, not mandatory)
- Bytes in PayloadValueObject must be non-empty for dispatched messages

## Errors
| Error | Condition |
|---|---|
| `PayloadSchemaAlreadyRegisteredError` | Schema for typeRef+version already active |
| `PayloadSchemaMissingSchemaRefError` | Avro/Protobuf payload without SchemaRef |
| `UnknownPayloadTypeRefError` | TypeRef not registered in schema catalog |

## Commands
| Command | Description |
|---|---|
| `RegisterPayloadSchema` | Register a new payload schema contract |
| `DeprecatePayloadSchema` | Deprecate a payload schema contract |

## Queries
| Query | Returns |
|---|---|
| `GetPayloadSchema(typeRef, version)` | Single payload schema contract |
| `ListPayloadSchemas(encoding?)` | Filtered schema list |

## Kafka Topic
`whyce.platform.envelope.payload.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for PayloadSchemaId
- `platform-system/schema/schema-definition` — SchemaRef target (cross-context reference via ID only)
