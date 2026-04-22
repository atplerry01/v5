# platform-system / envelope / message-envelope

**Classification:** platform-system  
**Context:** envelope  
**Domain:** message-envelope

## Purpose
The universal, transport-agnostic message wrapper. Composes a header, payload, and
metadata value object into a single coherent message unit, applicable to any message
kind (command, event, notification, query) regardless of transport mechanism.

## Aggregate: MessageEnvelopeAggregate

**Identity:** `EnvelopeId` — SHA256 of (header.messageId + payload.typeRef + metadata.correlationId)

**State:**
| Field | Type | Description |
|---|---|---|
| `EnvelopeId` | value object (Guid) | Deterministic envelope identifier |
| `Header` | value object (HeaderValueObject) | Routing, tracing, and content-type context |
| `Payload` | value object (PayloadValueObject) | Typed, encoded, schema-referenced payload |
| `Metadata` | value object (MessageMetadataValueObject) | Correlation, causation, version, issuedAt |
| `MessageKind` | enum value object | Command \| Event \| Notification \| Query |
| `Status` | enum | Created → Dispatched → Acknowledged \| Rejected |

## Events
| Event | Trigger |
|---|---|
| `MessageEnvelopeCreatedEvent` | Envelope created with valid header + payload + metadata |
| `MessageEnvelopeDispatchedEvent` | Envelope dispatched to transport |
| `MessageEnvelopeAcknowledgedEvent` | Dispatch acknowledged by destination |
| `MessageEnvelopeRejectedEvent` | Dispatch rejected by destination or transport |

## Invariants
- EnvelopeId is deterministic from (header.messageId + payload.typeRef + metadata.correlationId)
- Header, Payload, and Metadata are all non-null
- MessageKind is explicit — never inferred
- Status transitions: Created → Dispatched → Acknowledged | Rejected (terminal states irreversible)
- Dispatched → Acknowledged/Rejected is one-way (no reversion)
- Envelope structure is stable across transports — transport-agnostic by design

## Errors
| Error | Condition |
|---|---|
| `MessageEnvelopeAlreadyDispatchedError` | Dispatch attempted on already-dispatched envelope |
| `MessageEnvelopeInvalidHeaderError` | Header fails structural validation |
| `MessageEnvelopeInvalidPayloadError` | Payload TypeRef is empty or encoding is unknown |
| `MessageEnvelopeAlreadyTerminatedError` | Action on acknowledged/rejected envelope |

## Commands
| Command | Description |
|---|---|
| `CreateMessageEnvelope` | Create envelope with header, payload, metadata, and messageKind |
| `DispatchMessageEnvelope` | Transition to Dispatched status |
| `AcknowledgeMessageEnvelope` | Transition to Acknowledged (terminal) |
| `RejectMessageEnvelope` | Transition to Rejected (terminal) with reason |

## Queries
| Query | Returns |
|---|---|
| `GetMessageEnvelope(envelopeId)` | Single envelope with all value objects |
| `ListMessageEnvelopes(status?, messageKind?)` | Filtered envelope list |

## Projection Needs
`MessageEnvelopeView`: envelopeId, messageKind, status, correlationId, causationId, issuedAt

## Kafka Topic
`whyce.platform.envelope.message-envelope.events`

## Dependencies
- `core-system/identifier` — IIdGenerator for EnvelopeId
- `core-system/temporal` — IClock for IssuedAt in metadata
- `envelope/header` — HeaderValueObject definition
- `envelope/payload` — PayloadValueObject definition
- `envelope/metadata` — MessageMetadataValueObject definition
