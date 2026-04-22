# platform-system / envelope / header

**Classification:** platform-system  
**Context:** envelope  
**Domain:** header

## Purpose
Owns the canonical header structure for any message: routing addresses, distributed
tracing context, content-type descriptor, and message-kind hint. The header enables
transport-layer routing and observability without carrying business meaning.

## Primary Usage: Value Object (embedded in message-envelope)

`HeaderValueObject` is the primary artifact consumed by `message-envelope`:

| Field | Type | Description |
|---|---|---|
| `MessageId` | Guid (deterministic) | Unique ID for this specific message |
| `ContentType` | string | MIME type e.g. `application/json`, `application/avro` |
| `MessageKindHint` | string | `command` \| `event` \| `notification` \| `query` |
| `SourceAddress` | DomainRoute | Three-tuple (classification, context, domain) |
| `DestinationAddress` | DomainRoute (nullable) | Target route; null for broadcasts |
| `TraceId` | string | Distributed trace identifier (W3C TraceContext format) |
| `SpanId` | string | Current span identifier |
| `ParentSpanId` | string (nullable) | Parent span for hierarchical tracing |
| `SamplingFlag` | bool | Whether this trace is sampled for collection |

## Aggregate: HeaderSchemaAggregate (schema registration)

Used to register canonical header schema versions for governance purposes.

**Identity:** `HeaderSchemaId` — SHA256 of (headerKind + version)

**State:**
| Field | Type | Description |
|---|---|---|
| `HeaderSchemaId` | value object (Guid) | Deterministic schema ID |
| `HeaderKind` | value object (string) | Canonical header type name e.g. `whyce.standard.v1` |
| `RequiredFields` | collection | Field names that must be present |
| `Version` | int | Schema version |
| `Status` | enum | Active \| Deprecated |

## Events
| Event | Trigger |
|---|---|
| `HeaderSchemaRegisteredEvent` | New header schema version registered |
| `HeaderSchemaDeprecatedEvent` | Header schema version deprecated |

## Invariants
- HeaderSchemaId deterministic from (headerKind + version)
- RequiredFields must include at minimum: MessageId, ContentType, SourceAddress
- Published header schemas are immutable
- TraceId format follows W3C TraceContext specification

## Errors
| Error | Condition |
|---|---|
| `HeaderSchemaAlreadyRegisteredError` | Schema for headerKind+version already active |
| `HeaderSchemaMissingRequiredFieldError` | RequiredFields missing a mandatory entry |

## Commands
| Command | Description |
|---|---|
| `RegisterHeaderSchema` | Register a new header schema version |
| `DeprecateHeaderSchema` | Deprecate a header schema version |

## Queries
| Query | Returns |
|---|---|
| `GetHeaderSchema(headerKind, version)` | Single header schema |

## Kafka Topic
`whyce.platform.envelope.header.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for HeaderSchemaId
