# platform-system / schema / serialization

**Classification:** platform-system  
**Context:** schema  
**Domain:** serialization

## Purpose
Owns serialization format specifications — declares encoding, schema mapping, serialization
options, and round-trip integrity guarantees for converting messages to/from bytes.
Serialization formats are registered explicitly; encoding is never inferred at runtime.

## Aggregate: SerializationFormatAggregate

**Identity:** `SerializationFormatId` — SHA256 of (formatName + encoding + version)

**State:**
| Field | Type | Description |
|---|---|---|
| `SerializationFormatId` | value object (Guid) | Deterministic format ID |
| `FormatName` | value object (string) | Canonical format label e.g. `whyce.json.v1`, `whyce.avro.v1` |
| `Encoding` | enum value object | Json \| Avro \| Protobuf \| MsgPack \| Binary |
| `SchemaRef` | value object (Guid, nullable) | Required for Avro and Protobuf; recommended for Json |
| `Options` | collection (SerializationOption) | Structural serialization settings |
| `RoundTripGuarantee` | enum value object | Lossless \| LossyWithDocumentedFields |
| `Version` | int | Format version |
| `Status` | enum | Active \| Deprecated |

## Value Object: SerializationOption
| Field | Type | Description |
|---|---|---|
| `Key` | string | Option key e.g. `dateFormat`, `nullHandling`, `fieldNamingStrategy` |
| `Value` | string | Option value e.g. `ISO8601`, `omitNull`, `camelCase` |

Options are STRUCTURAL settings only — no business logic encoded in serialization options.

## Events
| Event | Trigger |
|---|---|
| `SerializationFormatRegisteredEvent` | New serialization format registered |
| `SerializationFormatDeprecatedEvent` | Serialization format deprecated |

## Invariants
- SerializationFormatId deterministic from (formatName + encoding + version)
- FormatName non-empty
- Encoding is explicit — never inferred at runtime
- Avro and Protobuf formats MUST carry a non-null SchemaRef
- RoundTripGuarantee is explicit — Lossless by default
- LossyWithDocumentedFields requires at least one documenting SerializationOption
  (key=`lossyField`, value=`{fieldName}:{reason}`) per lossy field
- Published serialization formats are immutable (new version = new aggregate)
- Options are structural only — no business semantics encoded

## Round-Trip Guarantee Semantics
- `Lossless`: Full serialize→deserialize cycle produces byte-equal or semantically
  equivalent object. Required for all Avro and Protobuf formats.
- `LossyWithDocumentedFields`: Some fields may not survive round-trip (e.g. high-precision
  float truncated in JSON). Each lossy field MUST be documented in Options.

## Errors
| Error | Condition |
|---|---|
| `SerializationFormatAlreadyRegisteredError` | Format for same (formatName, version) already active |
| `SerializationFormatMissingSchemaRefError` | Avro/Protobuf format registered without SchemaRef |
| `UndocumentedLossyFieldError` | LossyWithDocumentedFields declared without lossyField option |
| `SerializationFormatDeprecatedError` | Action attempted on deprecated format |

## Commands
| Command | Description |
|---|---|
| `RegisterSerializationFormat` | Register a new serialization format |
| `DeprecateSerializationFormat` | Deprecate a serialization format |

## Queries
| Query | Returns |
|---|---|
| `GetSerializationFormat(formatName, version)` | Single serialization format |
| `ListSerializationFormats(encoding?)` | Filtered format list |

## Projection Needs
`SerializationFormatView`: serializationFormatId, formatName, encoding, schemaRef, roundTripGuarantee, options[], version, status

## Kafka Topic
`whyce.platform.schema.serialization.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for SerializationFormatId
- `schema/schema-definition` — SchemaRef target (cross-domain reference via ID only)
