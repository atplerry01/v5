---
TITLE: platform-system / envelope — E1→EX Canonical Batch
CLASSIFICATION: platform-system
CONTEXT: envelope
DOMAIN GROUP: envelope (3-level form — no physical domain-group folder segment)
DOMAINS:
  - message-envelope
  - header
  - payload
  - metadata
BATCH_DESCRIPTION: >
  Defines the transport-agnostic message wrapping structure shared across all
  communication contracts. The envelope context is a cross-cutting structural layer:
  it defines the universal wrapper (message-envelope), routing/tracing header (header),
  opaque payload contract (payload), and cross-cutting metadata (metadata) that apply
  to any message type regardless of whether it carries a command or an event.
  This context is ENTIRELY NEW — it does not currently exist in the domain.
SOURCE: claude/project-topics/v3/platform-system.md
TEMPLATE: claude/templates/pipelines/generic-prompt.md
---

# WBSM $3 MANDATORY SECTIONS

## TITLE
platform-system / envelope — E1→EX Canonical Domain Batch (NEW CONTEXT)

## CONTEXT
The `envelope` context of platform-system defines the transport-agnostic message structure
that is common to both commands and events. While command/command-envelope and
event/event-envelope define type-specific envelopes, the `envelope` context defines the
GENERAL envelope model — usable by any message type.

This is distinct from command-envelope and event-envelope:
- command-envelope: type-specific envelope for commands with command-specific fields
- event-envelope: type-specific envelope for events with event-specific fields
- message-envelope (this context): the abstract, transport-agnostic message wrapper
  that can represent any structured message regardless of type

Current state: CONTEXT DOES NOT EXIST — full new delivery required.
Physical path: `src/domain/platform-system/envelope/{domain}/`
Form: 3-level (no domain-group segment)

Platform-system README currently lists only 4 contexts. The `envelope` context must be
added to the README as the 5th canonical context.

## OBJECTIVE
1. Create the entire `envelope` context from scratch.
2. Implement all 4 domains: message-envelope, header, payload, metadata.
3. Deliver full E1→EX.

## CONSTRAINTS
- envelope context depends only on core-system (identifier + temporal)
- No business semantics — pure structural/protocol model
- Transport-agnostic — no Kafka, HTTP, or gRPC types in domain layer
- No Guid.NewGuid(), no DateTime.UtcNow — IIdGenerator + IClock
- Domain layer: zero external dependencies
- message-envelope is NOT the same as command-envelope or event-envelope
- metadata in this context = cross-cutting message metadata (correlation, causation,
  version) — NOT the same as command-metadata or event-metadata (which are type-specific)

## EXECUTION STEPS

### STAGE E1 — DOMAIN MODEL

#### message-envelope
Purpose: The universal, transport-agnostic message wrapper. Composes header + payload + metadata
into a single coherent message unit regardless of message type.

Aggregate: MessageEnvelopeAggregate
- EnvelopeId (value object, SHA256 of header.messageId + payload.typeRef + metadata.correlationId)
- Header (value object, composition of HeaderId + routing context + tracing context)
- Payload (value object, composition of TypeRef + encoding + schemaRef + bytes)
- Metadata (value object, composition of correlationId + causationId + version + issuedAt)
- MessageKind: Command | Event | Notification | Query (enum value object)
- Status: Created | Dispatched | Acknowledged | Rejected
Events:
- MessageEnvelopeCreatedEvent
- MessageEnvelopeDispatchedEvent
- MessageEnvelopeAcknowledgedEvent
- MessageEnvelopeRejectedEvent
Invariants:
- EnvelopeId is deterministic and replay-stable
- Header, Payload, Metadata are all non-null
- MessageKind is explicit
- Dispatched → Acknowledged | Rejected (terminal states are irreversible)
Errors:
- MessageEnvelopeAlreadyDispatchedError
- MessageEnvelopeInvalidHeaderError
- MessageEnvelopeInvalidPayloadError

#### header
Purpose: Message header model — carries routing context, distributed tracing context,
content-type metadata, and identity tokens sufficient to route and trace any message.

Aggregate: HeaderAggregate (NOTE: headers are usually immutable value objects;
  however, for DDD registration/schema purposes, we model the canonical header structure
  as an aggregate to allow lifecycle tracking of header templates/standards)
  
Value Object (primary usage — embedded in message-envelope):
HeaderValueObject:
- MessageId (value object, deterministic ID for this specific message)
- ContentType (value object, MIME type string e.g. "application/json")
- MessageKindHint (string — "command" | "event" | "notification")
- SourceAddress (DomainRoute value object)
- DestinationAddress (DomainRoute value object — optional for broadcasts)
- TraceId (string — distributed trace identifier)
- SpanId (string — current span)
- ParentSpanId (string — optional)
- SamplingFlag (bool)

Aggregate: HeaderSchemaAggregate (for schema registration purposes)
- HeaderSchemaId (SHA256 of headerKind+version)
- HeaderKind (string — canonical header type name)
- RequiredFields (collection of string field names)
- Version (int)
Events:
- HeaderSchemaRegisteredEvent
- HeaderSchemaDeprecatedEvent
Invariants:
- HeaderSchemaId deterministic
- RequiredFields includes at minimum: messageId, contentType, sourceAddress

#### payload
Purpose: Opaque payload contract — defines how payload bytes/JSON are typed,
encoded, and schema-referenced. The payload itself is opaque; this domain owns
the CONTRACT for describing a payload.

Aggregate: PayloadSchemaAggregate (canonical payload contract registration)
- PayloadSchemaId (SHA256 of typeRef+encoding+schemaRef+version)
- TypeRef (value object, string — e.g. "platform.command.RegisterCommandDefinition.v1")
- Encoding (value object enum: Json | Avro | Protobuf | Binary)
- SchemaRef (string — reference to platform-system/schema/schema-definition ID)
- Version (int)
- MaxSizeBytes (value object, long — optional size cap)
Events:
- PayloadSchemaRegisteredEvent
- PayloadSchemaDeprecatedEvent

Value Object (primary usage — embedded in message-envelope):
PayloadValueObject:
- TypeRef (string)
- Encoding (enum)
- SchemaRef (string)
- Bytes (ReadOnlyMemory<byte> — opaque)

Invariants:
- PayloadSchemaId deterministic
- TypeRef non-empty
- Encoding is explicit (never inferred at runtime per schema standard)
- Bytes non-empty for dispatched payloads
Errors:
- PayloadSchemaAlreadyRegisteredError, UnknownPayloadTypeRefError

#### metadata
Purpose: Cross-cutting message metadata — correlation chain, causation chain, versioning,
and issuance timestamp. These fields apply universally to any message type.

This is NOT the same as command-metadata or event-metadata (type-specific).
This metadata applies at the general message-envelope level.

Aggregate: MessageMetadataSchemaAggregate (for canonical metadata field registration)
- MetadataSchemaId (SHA256 of schemaVersion)
- SchemaVersion (int)
- RequiredFields (collection: correlationId, causationId, issuedAt, messageVersion)
- OptionalFields (collection: tenantId, sessionId, etc.)
Events:
- MessageMetadataSchemaRegisteredEvent

Value Object (primary usage — embedded in message-envelope):
MessageMetadataValueObject:
- CorrelationId (Guid value object — propagated from originating command)
- CausationId (Guid value object — ID of the immediately causing message)
- MessageVersion (int — message schema version)
- IssuedAt (IClock value object)
- TenantId (optional Guid value object)

Invariants:
- CorrelationId non-empty
- CausationId non-empty (except for root commands which set it to their own ID)
- IssuedAt from IClock
- MessageVersion >= 1
Errors:
- MissingCorrelationIdError, MissingCausationIdError

### STAGE E2 — COMMAND LAYER
message-envelope: CreateMessageEnvelope, DispatchMessageEnvelope, AcknowledgeMessageEnvelope, RejectMessageEnvelope
header: RegisterHeaderSchema, DeprecateHeaderSchema
payload: RegisterPayloadSchema, DeprecatePayloadSchema
metadata: RegisterMessageMetadataSchema

### STAGE E3 — QUERY LAYER
GetMessageEnvelope(envelopeId), ListMessageEnvelopes(status?, messageKind?)
GetHeaderSchema(headerKind, version)
GetPayloadSchema(typeRef, version)
GetMessageMetadataSchema(version)

### STAGE E4 — T2E ENGINE HANDLERS
Standard T2E handlers with IEventStore idempotency probe, IIdGenerator, IClock injection.

### STAGE E5 — POLICY INTEGRATION
No policy ownership in envelope context.
Policy action names (informational): platform.envelope.message.create, platform.envelope.message.dispatch

### STAGE E6 — EVENT FABRIC
Topics:
- whyce.platform.envelope.message-envelope.events
- whyce.platform.envelope.header.events
- whyce.platform.envelope.payload.events
- whyce.platform.envelope.metadata.events

### STAGE E7 — PROJECTIONS
- MessageEnvelopeView (envelopeId, messageKind, status, correlationId, issuedAt)
- HeaderSchemaView (headerKind, version, requiredFields[], status)
- PayloadSchemaView (typeRef, encoding, schemaRef, version, status)
- MessageMetadataSchemaView (schemaVersion, requiredFields[], optionalFields[])

### STAGE E8 — API LAYER
POST/GET /api/platform/envelope/messages
POST/GET /api/platform/envelope/messages/{id}/dispatch
POST/GET /api/platform/envelope/header-schemas
POST/GET /api/platform/envelope/payload-schemas
POST/GET /api/platform/envelope/metadata-schemas

### STAGE E9 — WORKFLOW
No T1M workflow. All direct T2E.

### STAGE E10 — OBSERVABILITY
Metrics: envelope.message.created, envelope.message.dispatched, envelope.message.acknowledged,
envelope.message.rejected (all whyce.platform.envelope.* prefix)

### STAGE E11 — SECURITY
Service identity required. No domain-level access control.

### STAGE E12 — E2E VALIDATION
- Register header schema → payload schema → create message-envelope with both references
- Dispatch envelope → verify status transitions through Created → Dispatched → Acknowledged
- Verify correlationId/causationId propagation through metadata value object
- Verify Kafka events published on correct topics

## OUTPUT FORMAT
Per domain: aggregate, value objects, events, errors, specifications, README.md
Also: src/domain/platform-system/envelope/README.md (context README — new file)
Also: update src/domain/platform-system/README.md (add envelope context to table)

## VALIDATION CRITERIA
- [ ] src/domain/platform-system/envelope/ context folder created
- [ ] All 4 domain README.md files created
- [ ] envelope/README.md created with purpose, domain table, dependencies
- [ ] platform-system/README.md updated to list envelope as 5th context
- [ ] message-envelope, header, payload, metadata domain models defined
- [ ] No overlap with command-envelope or event-envelope (distinct structural level)
- [ ] Correlation/causation chain correctly modeled in metadata value object
- [ ] Topics follow whyce.platform.envelope.{domain}.events pattern
- [ ] No Guid.NewGuid(), no DateTime.UtcNow
