---
TITLE: platform-system / command — E1→EX Canonical Batch
CLASSIFICATION: platform-system
CONTEXT: command
DOMAIN GROUP: command (3-level form — no physical domain-group folder segment)
DOMAINS:
  - command-definition
  - command-envelope
  - command-metadata
  - command-routing
BATCH_DESCRIPTION: >
  Defines the canonical intent representation for system-wide command dispatch.
  Covers command type registration, the envelope structure wrapping command payloads,
  structured metadata carried alongside the envelope, and routing rules that bind
  command types to handler addresses deterministically.
SOURCE: claude/project-topics/v3/platform-system.md
TEMPLATE: claude/templates/pipelines/generic-prompt.md
---

# WBSM $3 MANDATORY SECTIONS

## TITLE
platform-system / command — E1→EX Canonical Domain Batch

## CONTEXT
platform-system is the canonical communication contract classification of the WBSM v3
architecture. It defines HOW systems talk, not WHAT they mean. It is business-agnostic,
policy-agnostic, and protocol-level only.

The `command` context owns the canonical structural model of a command:
- how a command is identified, typed, and structured (command-definition)
- how it is packaged for transport (command-envelope)
- what metadata accompanies it for tracing, identity, and policy hooks (command-metadata)
- how it is directed to its handler (command-routing)

Current state:
- command-definition: ✓ scaffolded
- command-envelope: ✓ scaffolded
- command-routing: ✓ scaffolded
- command-metadata: MISSING — must be added per platform-system topic

Physical path: `src/domain/platform-system/command/{domain}/`
Form: 3-level (classification/context/domain — no domain-group segment)

## OBJECTIVE
1. Add the missing `command-metadata` domain to align with the platform-system topic.
2. Update all 4 command context domains to platform-system topic specification.
3. Implement full E1→EX delivery (domain through API, tests, infrastructure) for the
   complete command context.

## CONSTRAINTS
- platform-system depends only on core-system (identifier + temporal primitives)
- No business semantics, no policy logic, no authorization logic
- No domain-specific naming inside platform domains
- Domain layer: zero external dependencies (no DI, no EF, no HTTP)
- Determinism: IIdGenerator + IClock — no Guid.NewGuid(), no DateTime.UtcNow
- All aggregates must emit domain events on state change
- All IDs: SHA256-derived via IIdGenerator with deterministic seed
- 3-level form: do NOT introduce a domain-group folder segment
- DS-R1: classification folder uses -system suffix (already in place)
- DS-R2: non-domain layers use raw classification name without -system

## EXECUTION STEPS

### STAGE E1 — DOMAIN MODEL

#### command-definition
Purpose: Registered command type definitions — canonical registry entry for a command type.

Aggregate: CommandDefinitionAggregate
- CommandDefinitionId (value object, SHA256 of name+version)
- CommandTypeName (value object, non-empty string)
- SchemaRef (value object, references platform-system/schema/schema-definition)
- HandlerAddress (value object, DomainRoute three-tuple)
- Version (int, monotonically incremented)
- Status: Active | Deprecated
Events:
- CommandDefinitionRegisteredEvent
- CommandDefinitionDeprecatedEvent
Invariants:
- CommandTypeName is unique within the registry
- SchemaRef must be non-null and non-empty
- HandlerAddress is a valid DomainRoute
- Deprecated definitions cannot be re-activated (compensation only)
Errors:
- CommandDefinitionAlreadyRegisteredError
- CommandDefinitionNotFoundError
- InvalidHandlerAddressError

#### command-envelope
Purpose: Transport wrapper for a command payload — carries identity, type, and routing context.

Aggregate: CommandEnvelopeAggregate
- EnvelopeId (value object, SHA256 of type+source+correlationId+issuedAt)
- CommandType (value object, references command-definition name)
- SourceRoute (value object, DomainRoute)
- DestinationRoute (value object, DomainRoute)
- CorrelationId (value object, Guid)
- CausationId (value object, Guid — optional)
- IssuedAt (value object, from IClock)
- Payload (value object, opaque bytes/JSON — schema-referenced)
Events:
- CommandEnvelopeCreatedEvent
Invariants:
- EnvelopeId is deterministic
- SourceRoute must be non-empty
- CommandType must match a registered command-definition
- Payload is non-null
Errors:
- InvalidCommandTypeError
- EnvelopeIdCollisionError

#### command-metadata
Purpose: Structured metadata attached to a command envelope for tracing, policy hooks, and identity.
This domain is the NEW addition per platform-system topic.

Aggregate: CommandMetadataAggregate
- MetadataId (value object, SHA256 of envelopeId+actorId+traceId)
- EnvelopeRef (value object, references CommandEnvelopeId)
- ActorId (value object, from trust-system — opaque ID string)
- TraceId (value object, distributed trace identifier)
- SpanId (value object, current span)
- PolicyContextRef (value object, policyId + policyVersion — opaque, for policy hook)
- TrustScore (value object, numeric 0..100)
- IssuedAt (value object, from IClock)
Events:
- CommandMetadataAttachedEvent
Invariants:
- MetadataId is deterministic
- EnvelopeRef must be non-empty
- ActorId must be non-empty
- TrustScore in [0, 100]
- PolicyContextRef is structurally valid if present
Errors:
- CommandMetadataEnvelopeRefMissingError
- InvalidTrustScoreError
Note: CommandMetadata carries NO policy evaluation logic — it is a structural carrier only.
Policy evaluation occurs in the runtime layer.

#### command-routing
Purpose: Routing rules that bind a command type to a specific handler address deterministically.

Aggregate: CommandRoutingAggregate
- RoutingRuleId (value object, SHA256 of commandTypeName+handlerAddress)
- CommandTypeName (value object, references command-definition)
- HandlerAddress (value object, DomainRoute three-tuple)
- Priority (value object, int — used for ordered resolution when multiple rules exist)
- Status: Active | Inactive
Events:
- CommandRoutingRuleRegisteredEvent
- CommandRoutingRuleDeactivatedEvent
Invariants:
- RoutingRuleId is deterministic
- CommandTypeName maps to exactly one Active routing rule at any time
- HandlerAddress is a valid DomainRoute
- Priority must be unique when multiple rules target the same command type
Errors:
- CommandRoutingRuleAlreadyActiveError
- AmbiguousRoutingError
- InvalidHandlerAddressError

### STAGE E2 — COMMAND LAYER
Commands (per domain):
command-definition: RegisterCommandDefinition, DeprecateCommandDefinition
command-envelope: CreateCommandEnvelope
command-metadata: AttachCommandMetadata
command-routing: RegisterCommandRoutingRule, DeactivateCommandRoutingRule

### STAGE E3 — QUERY LAYER
Queries:
- GetCommandDefinition(commandTypeName, version)
- ListCommandDefinitions(status?)
- GetCommandEnvelope(envelopeId)
- GetCommandMetadata(metadataId), GetCommandMetadataByEnvelopeRef(envelopeRef)
- GetCommandRoutingRule(commandTypeName), ListCommandRoutingRules(status?)

### STAGE E4 — T2E ENGINE HANDLERS
One engine handler per command. All handlers:
- Inject IEventStore for idempotency probe
- Inject IIdGenerator (deterministic seeds from command fields)
- Inject IClock
- Emit domain events via context.EmitEvents(...)

### STAGE E5 — POLICY INTEGRATION
platform-system has no policy ownership. Policy hooks are structural:
- CommandMetadataAggregate carries PolicyContextRef (opaque — no evaluation)
- Policy gate runs in runtime pipeline before reaching engine handlers
- Policy action names (informational): platform.command.definition.register,
  platform.command.routing.register

### STAGE E6 — EVENT FABRIC
Topic naming (canonical Kafka format):
- whyce.platform.command.command-definition.events
- whyce.platform.command.command-envelope.events
- whyce.platform.command.command-metadata.events
- whyce.platform.command.command-routing.events

### STAGE E7 — PROJECTIONS
Read models:
- CommandDefinitionView (commandTypeName, schemaRef, handlerAddress, status, version)
- CommandEnvelopeView (envelopeId, commandType, source, destination, correlationId, issuedAt)
- CommandMetadataView (metadataId, envelopeRef, actorId, traceId, trustScore, issuedAt)
- CommandRoutingView (commandTypeName → handlerAddress, priority, status)

### STAGE E8 — API LAYER
Endpoints:
POST   /api/platform/command/definitions
GET    /api/platform/command/definitions/{name}/{version}
DELETE /api/platform/command/definitions/{name}/{version}    (deprecate)
POST   /api/platform/command/envelopes
GET    /api/platform/command/envelopes/{envelopeId}
POST   /api/platform/command/metadata
GET    /api/platform/command/metadata/{metadataId}
POST   /api/platform/command/routing-rules
DELETE /api/platform/command/routing-rules/{commandTypeName}  (deactivate)
GET    /api/platform/command/routing/{commandTypeName}

### STAGE E9 — WORKFLOW
No T1M workflow needed. All operations are direct T2E.

### STAGE E10 — OBSERVABILITY
Metrics:
- whyce.platform.command.definition.registered.total
- whyce.platform.command.envelope.created.total
- whyce.platform.command.metadata.attached.total
- whyce.platform.command.routing.rule.registered.total
Traces: span per command-handler execution, tagged with commandType

### STAGE E11 — SECURITY
Actor context required for all write operations.
No access control decisions in platform-system — these are structural registrations.
Platform.command is internal infrastructure — exposed only to authenticated service identities.

### STAGE E12 — E2E VALIDATION
- Register a command definition → verify event persisted + projected
- Create envelope for registered type → verify deterministic ID
- Attach metadata with valid trustScore → verify metadata event
- Register routing rule → verify routing resolution returns correct handler
- Kafka: verify events appear on correct topics
- Projection: verify read models updated after events consumed

## OUTPUT FORMAT
Per domain:
1. Aggregate class (production-ready C#)
2. Value objects
3. Domain events
4. Errors
5. Specifications (where applicable)
6. README.md update

## VALIDATION CRITERIA
- [ ] command-metadata domain exists at src/domain/platform-system/command/command-metadata/
- [ ] All 4 domains have aggregate + events + errors + value-objects
- [ ] No Guid.NewGuid(), no DateTime.UtcNow anywhere in domain code
- [ ] No DI, EF, HTTP references in domain layer
- [ ] All IDs are SHA256-derived via IIdGenerator
- [ ] All events are past-tense named
- [ ] Routing invariant: one active rule per command type enforced
- [ ] Metadata carries no policy evaluation logic
- [ ] Topic names follow whyce.platform.command.{domain}.events pattern
- [ ] command/README.md updated to list all 4 domains including command-metadata
