# platform-system / command / command-metadata

**Classification:** platform-system  
**Context:** command  
**Domain:** command-metadata

## Purpose
Owns the structural model for metadata attached to a command envelope. Provides the
tracing context, identity reference, policy context hook, and trust score that the
runtime pipeline stamps onto a command before dispatch — without encoding any policy
evaluation logic itself.

command-metadata is a structural carrier only. Policy evaluation occurs in the runtime
pipeline before the command reaches the engine layer.

## Aggregate: CommandMetadataAggregate

**Identity:** `MetadataId` — SHA256 of (envelopeId + actorId + traceId)

**State:**
| Field | Type | Description |
|---|---|---|
| `MetadataId` | value object (Guid) | Deterministic metadata identifier |
| `EnvelopeRef` | value object (Guid) | Reference to the CommandEnvelope this metadata annotates |
| `ActorId` | value object (string) | Opaque actor identifier from trust-system |
| `TraceId` | value object (string) | Distributed trace ID (propagated from inbound request) |
| `SpanId` | value object (string) | Current span identifier |
| `PolicyContextRef` | value object (string, nullable) | Opaque policyId + policyVersion hint for runtime hooks |
| `TrustScore` | value object (int 0–100) | Trust level of the actor at time of dispatch |
| `IssuedAt` | value object (DateTimeOffset) | Timestamp from IClock |

## Events
| Event | Trigger |
|---|---|
| `CommandMetadataAttachedEvent` | Metadata successfully attached to an envelope |

## Invariants
- MetadataId is deterministic (SHA256 of envelopeId+actorId+traceId)
- EnvelopeRef must be non-empty
- ActorId must be non-empty — no anonymous commands
- TrustScore must be in [0, 100]
- PolicyContextRef is structurally valid if present (non-empty policyId)
- IssuedAt must come from IClock — no DateTime.UtcNow
- No policy evaluation logic in this aggregate — structural read/carry only

## Errors
| Error | Condition |
|---|---|
| `CommandMetadataEnvelopeRefMissingError` | EnvelopeRef is empty |
| `CommandMetadataActorIdMissingError` | ActorId is empty or whitespace |
| `InvalidTrustScoreError` | TrustScore outside [0, 100] |

## Commands
| Command | Description |
|---|---|
| `AttachCommandMetadata` | Attach metadata record to a command envelope |

## Queries
| Query | Returns |
|---|---|
| `GetCommandMetadata(metadataId)` | Single metadata record |
| `GetCommandMetadataByEnvelopeRef(envelopeRef)` | Metadata for a given envelope |

## Projection Needs
`CommandMetadataView`: metadataId, envelopeRef, actorId, traceId, trustScore, issuedAt

## Kafka Topic
`whyce.platform.command.command-metadata.events`

## Dependencies
- `core-system/identifier` — deterministic ID generation via IIdGenerator
- `core-system/temporal` — IssuedAt via IClock
