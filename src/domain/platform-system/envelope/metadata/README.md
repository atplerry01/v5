# platform-system / envelope / metadata

**Classification:** platform-system  
**Context:** envelope  
**Domain:** metadata

## Purpose
Owns the cross-cutting message metadata model — the correlation chain, causation chain,
schema version, and issuance timestamp that apply universally to any message type.

This domain is distinct from `command/command-metadata` (command-type-specific: actor,
trust score, policy context) and `event/event-metadata` (event-type-specific: execution
hash, policy decision hash). The envelope metadata applies at the general message level.

## Primary Usage: Value Object (embedded in message-envelope)

`MessageMetadataValueObject` is the primary artifact consumed by `message-envelope`:

| Field | Type | Description |
|---|---|---|
| `CorrelationId` | Guid value object | Propagated from the originating command/request |
| `CausationId` | Guid value object | ID of the immediately causing message |
| `MessageVersion` | int | Schema version of the message payload (≥ 1) |
| `IssuedAt` | DateTimeOffset | From IClock — no DateTime.UtcNow |
| `TenantId` | Guid (nullable) | Optional tenant scope |

## Aggregate: MessageMetadataSchemaAggregate (metadata schema registration)

Used to register canonical metadata field schemas for governance purposes.

**Identity:** `MetadataSchemaId` — SHA256 of (schemaVersion)

**State:**
| Field | Type | Description |
|---|---|---|
| `MetadataSchemaId` | value object (Guid) | Deterministic schema ID |
| `SchemaVersion` | int | Metadata schema version |
| `RequiredFields` | collection (string) | Fields that must be present on every message |
| `OptionalFields` | collection (string) | Fields that may be present |

## Events
| Event | Trigger |
|---|---|
| `MessageMetadataSchemaRegisteredEvent` | New metadata schema version registered |

## Invariants
- MetadataSchemaId deterministic from (schemaVersion)
- RequiredFields must include: CorrelationId, CausationId, IssuedAt, MessageVersion
- CorrelationId must be non-empty on every message
- CausationId must be non-empty (root commands set it equal to their own command ID)
- MessageVersion must be ≥ 1
- IssuedAt must come from IClock — no DateTime.UtcNow

## Errors
| Error | Condition |
|---|---|
| `MissingCorrelationIdError` | CorrelationId is Guid.Empty |
| `MissingCausationIdError` | CausationId is Guid.Empty |
| `InvalidMessageVersionError` | MessageVersion < 1 |

## Commands
| Command | Description |
|---|---|
| `RegisterMessageMetadataSchema` | Register a new metadata schema version |

## Queries
| Query | Returns |
|---|---|
| `GetMessageMetadataSchema(schemaVersion)` | Single metadata schema definition |

## Kafka Topic
`whyce.platform.envelope.metadata.events`

## Correlation Chain Semantics
- `CorrelationId`: Identifies the end-to-end business transaction. Copied unchanged from
  the originating command through all downstream events and commands.
- `CausationId`: Identifies the immediate predecessor in the causation chain. A command's
  CausationId is the command that caused it; an event's CausationId is the command that
  produced it.

## Dependencies
- `core-system/identifier` — IIdGenerator for MetadataSchemaId
- `core-system/temporal` — IClock for IssuedAt
