---
TITLE: platform-system / event — E1→EX Canonical Batch
CLASSIFICATION: platform-system
CONTEXT: event
DOMAIN GROUP: event (3-level form — no physical domain-group folder segment)
DOMAINS:
  - event-definition
  - event-schema
  - event-envelope
  - event-metadata
  - event-stream
BATCH_DESCRIPTION: >
  Defines canonical fact representation for system-wide event propagation.
  Covers event type registration, canonical payload schema structure, the envelope
  wrapping event payloads, structured metadata for tracing and policy provenance,
  and event stream definitions for ordered event sequences.
SOURCE: claude/project-topics/v3/platform-system.md
TEMPLATE: claude/templates/pipelines/generic-prompt.md
---

# WBSM $3 MANDATORY SECTIONS

## TITLE
platform-system / event — E1→EX Canonical Domain Batch

## CONTEXT
The `event` context of platform-system owns the canonical structural model of a domain event:
- how an event type is registered and versioned (event-definition)
- the canonical payload schema for an event type (event-schema)
- the transport wrapper around an event payload (event-envelope)
- metadata for tracing, execution provenance, and policy hooks (event-metadata)
- ordered event sequences declared from a source DomainRoute (event-stream)

Current state:
- event-definition: ✓ scaffolded
- event-schema: ✓ scaffolded
- event-envelope: ✓ scaffolded
- event-stream: ✓ scaffolded
- event-metadata: MISSING — must be added per platform-system topic

Physical path: `src/domain/platform-system/event/{domain}/`
Form: 3-level (no domain-group segment)

## OBJECTIVE
1. Add the missing `event-metadata` domain per platform-system topic.
2. Update all 5 event context domains to full specification.
3. Deliver complete E1→EX for the event context.

## CONSTRAINTS
- platform-system depends only on core-system (identifier + temporal)
- Events represent facts (past tense) — IMMUTABLE once emitted
- Events must be versioned; replay-safe
- No business semantics, no policy evaluation logic
- No Guid.NewGuid(), no DateTime.UtcNow — use IIdGenerator + IClock
- Domain layer: zero external dependencies
- event-metadata carries NO policy evaluation — structural carrier only

## EXECUTION STEPS

### STAGE E1 — DOMAIN MODEL

#### event-definition
Purpose: Registered event type definitions — canonical registry entry for a fact type.

Aggregate: EventDefinitionAggregate
- EventDefinitionId (SHA256 of name+version)
- EventTypeName (non-empty string value object)
- SchemaRef (references platform-system/schema/schema-definition by ID)
- StreamRef (references event-stream by name — optional)
- Version (int)
- Status: Active | Deprecated
Events:
- EventDefinitionRegisteredEvent
- EventDefinitionDeprecatedEvent
Invariants:
- EventTypeName unique per version
- SchemaRef non-null
- Deprecated definitions are immutable
Errors:
- EventDefinitionAlreadyRegisteredError, EventDefinitionNotFoundError

#### event-schema
Purpose: Canonical schema structure for an event payload.

Aggregate: EventSchemaAggregate
- EventSchemaId (SHA256 of eventTypeName+version)
- EventTypeName (references event-definition)
- Fields (collection of FieldDescriptor value objects: name, type, required, description)
- Version (int)
- CompatibilityMode: Backward | Forward | Full | None
- Status: Active | Deprecated
Events:
- EventSchemaRegisteredEvent
- EventSchemaDeprecatedEvent
Invariants:
- EventSchemaId is deterministic
- At least one required field declared
- CompatibilityMode is explicit
- Published schemas are immutable (new version = new aggregate instance)
Errors:
- EventSchemaAlreadyRegisteredError, IncompatibleSchemaEvolutionError

#### event-envelope
Purpose: Transport-agnostic wrapper for an event payload.

Aggregate: EventEnvelopeAggregate
- EnvelopeId (SHA256 of eventType+aggregateId+causationId+occurredAt)
- EventType (references event-definition name)
- AggregateId (Guid value object)
- CorrelationId (Guid value object)
- CausationId (Guid value object — optional)
- OccurredAt (IClock-derived timestamp value object)
- Payload (opaque bytes/JSON — schema-referenced)
- SchemaVersion (int)
Events:
- EventEnvelopeCreatedEvent
Invariants:
- EnvelopeId is deterministic and replay-stable
- EventType must match a registered event-definition
- AggregateId non-empty
- Payload non-null
- Events are immutable once created — no state transition after creation
Errors:
- InvalidEventTypeError, EnvelopeIdCollisionError

#### event-metadata
Purpose: Structured metadata attached to an event envelope for tracing, audit, and policy provenance.
This is the NEW domain added per platform-system topic.

Aggregate: EventMetadataAggregate
- MetadataId (SHA256 of envelopeId+executionHash+policyDecisionHash)
- EnvelopeRef (references EventEnvelopeId)
- ExecutionHash (string — hash of the execution that produced this event)
- PolicyDecisionHash (string — hash of the policy decision authorizing this event)
- ActorId (opaque string — from trust-system identity)
- TraceId (string — distributed trace identifier)
- SpanId (string — current span)
- IssuedAt (IClock value object)
Events:
- EventMetadataAttachedEvent
Invariants:
- MetadataId is deterministic
- EnvelopeRef non-empty
- ExecutionHash non-empty (replay uses "replay" sentinel — tolerated)
- PolicyDecisionHash non-empty (replay uses "replay" sentinel — tolerated)
- ActorId non-empty
Note: This domain carries NO policy evaluation. It is a structural read of hashes
already computed by the runtime layer and stamped onto the event.
Errors:
- EventMetadataEnvelopeRefMissingError, MissingExecutionHashError

#### event-stream
Purpose: Ordered sequence definition for events emitted by a source DomainRoute.

Aggregate: EventStreamAggregate
- StreamId (SHA256 of streamName)
- StreamName (non-empty string value object)
- SourceRoute (DomainRoute three-tuple value object)
- RetentionPolicy (value object: duration + maxCount)
- OrderingGuarantee: PerPartitionOrdered | GloballyOrdered
- Status: Active | Archived
Events:
- EventStreamDeclaredEvent
- EventStreamArchivedEvent
Invariants:
- StreamId is deterministic
- StreamName unique per SourceRoute
- RetentionPolicy is explicit
- Archived streams receive no new events
Errors:
- EventStreamAlreadyDeclaredError, EventStreamArchivedError

### STAGE E2 — COMMAND LAYER
event-definition: RegisterEventDefinition, DeprecateEventDefinition
event-schema: RegisterEventSchema, DeprecateEventSchema
event-envelope: CreateEventEnvelope
event-metadata: AttachEventMetadata
event-stream: DeclareEventStream, ArchiveEventStream

### STAGE E3 — QUERY LAYER
GetEventDefinition, ListEventDefinitions
GetEventSchema(typeName, version), ListEventSchemas
GetEventEnvelope(envelopeId)
GetEventMetadata(metadataId), GetEventMetadataByEnvelopeRef(envelopeRef)
GetEventStream(streamName), ListEventStreams(sourceRoute?)

### STAGE E4 — T2E ENGINE HANDLERS
One engine handler per command. IEventStore probe for lifecycle-init idempotency.
IIdGenerator + IClock injected. All emit domain events via context.EmitEvents.

### STAGE E5 — POLICY INTEGRATION
No policy ownership. Policy hooks are structural:
- EventMetadataAggregate carries PolicyDecisionHash (opaque — no evaluation)
- Policy gate runs in runtime before engine
- Policy action names (informational): platform.event.definition.register,
  platform.event.stream.declare

### STAGE E6 — EVENT FABRIC
Topics:
- whyce.platform.event.event-definition.events
- whyce.platform.event.event-schema.events
- whyce.platform.event.event-envelope.events
- whyce.platform.event.event-metadata.events
- whyce.platform.event.event-stream.events

### STAGE E7 — PROJECTIONS
- EventDefinitionView (typeName, schemaRef, streamRef, status, version)
- EventSchemaView (typeName, version, fields[], compatibilityMode, status)
- EventEnvelopeView (envelopeId, eventType, aggregateId, correlationId, occurredAt)
- EventMetadataView (metadataId, envelopeRef, executionHash, policyDecisionHash, actorId)
- EventStreamView (streamName, sourceRoute, retentionPolicy, orderingGuarantee, status)

### STAGE E8 — API LAYER
POST/GET/DELETE /api/platform/event/definitions
POST/GET/DELETE /api/platform/event/schemas
POST/GET /api/platform/event/envelopes
POST/GET /api/platform/event/metadata
POST/GET/DELETE /api/platform/event/streams

### STAGE E9 — WORKFLOW
No T1M workflow. All direct T2E.

### STAGE E10 — OBSERVABILITY
Metrics: event.definition.registered, event.schema.registered, event.envelope.created,
event.metadata.attached, event.stream.declared (all whyce.platform.event.* prefix)

### STAGE E11 — SECURITY
Service identity required. No access control in platform-system layer.

### STAGE E12 — E2E VALIDATION
- Register event definition → schema → declare stream → create envelope → attach metadata
- Verify each step persists correct events and projects to read model
- Verify replay-sentinel tolerance in event-metadata (ExecutionHash="replay" accepted)
- Verify Kafka topics receive events in correct format

## OUTPUT FORMAT
Per domain: aggregate, value objects, events, errors, specifications, README.md

## VALIDATION CRITERIA
- [ ] event-metadata exists at src/domain/platform-system/event/event-metadata/
- [ ] All 5 domains have complete aggregate + events + errors + value-objects
- [ ] Events are immutable (no state mutation after EventEnvelopeCreatedEvent)
- [ ] Replay-sentinel tolerance declared in event-metadata invariants
- [ ] No Guid.NewGuid(), no DateTime.UtcNow
- [ ] event/README.md lists all 5 domains including event-metadata
- [ ] Topics follow whyce.platform.event.{domain}.events pattern
