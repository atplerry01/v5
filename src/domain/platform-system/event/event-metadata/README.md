# platform-system / event / event-metadata

**Classification:** platform-system  
**Context:** event  
**Domain:** event-metadata

## Purpose
Owns the structural model for metadata attached to an event envelope. Carries the
execution provenance (ExecutionHash), policy decision provenance (PolicyDecisionHash),
actor reference, and distributed tracing context — enabling event-level auditability
without encoding any policy evaluation logic.

event-metadata is a structural carrier. The ExecutionHash and PolicyDecisionHash values
are computed by the runtime layer and stamped here as opaque strings. During replay,
these fields carry the "replay" sentinel value per REPLAY-SENTINEL-PROTECTED-01.

## Aggregate: EventMetadataAggregate

**Identity:** `MetadataId` — SHA256 of (envelopeId + executionHash + policyDecisionHash)

**State:**
| Field | Type | Description |
|---|---|---|
| `MetadataId` | value object (Guid) | Deterministic metadata identifier |
| `EnvelopeRef` | value object (Guid) | Reference to the EventEnvelope this metadata annotates |
| `ExecutionHash` | value object (string) | Hash of the execution context that produced this event; "replay" during projection rebuild |
| `PolicyDecisionHash` | value object (string) | Hash of the policy decision authorizing this event; "replay" during projection rebuild |
| `ActorId` | value object (string) | Opaque actor identifier from trust-system |
| `TraceId` | value object (string) | Distributed trace ID |
| `SpanId` | value object (string) | Current span identifier |
| `IssuedAt` | value object (DateTimeOffset) | Timestamp from IClock |

## Events
| Event | Trigger |
|---|---|
| `EventMetadataAttachedEvent` | Metadata successfully attached to an event envelope |

## Invariants
- MetadataId is deterministic (SHA256 of envelopeId+executionHash+policyDecisionHash)
- EnvelopeRef must be non-empty
- ExecutionHash must be non-empty ("replay" sentinel is valid during projection rebuild)
- PolicyDecisionHash must be non-empty ("replay" sentinel is valid during projection rebuild)
- ActorId must be non-empty
- IssuedAt must come from IClock — no DateTime.UtcNow
- No policy evaluation logic — this aggregate is a structural read only

## Errors
| Error | Condition |
|---|---|
| `EventMetadataEnvelopeRefMissingError` | EnvelopeRef is empty |
| `EventMetadataActorIdMissingError` | ActorId is empty or whitespace |
| `MissingExecutionHashError` | ExecutionHash is null or empty |

## Commands
| Command | Description |
|---|---|
| `AttachEventMetadata` | Attach metadata record to an event envelope |

## Queries
| Query | Returns |
|---|---|
| `GetEventMetadata(metadataId)` | Single metadata record |
| `GetEventMetadataByEnvelopeRef(envelopeRef)` | Metadata for a given envelope |

## Projection Needs
`EventMetadataView`: metadataId, envelopeRef, executionHash, policyDecisionHash, actorId, traceId, issuedAt

## Kafka Topic
`whyce.platform.event.event-metadata.events`

## Replay Sentinel Tolerance
Per `REPLAY-SENTINEL-PROTECTED-01` (constitutional.guard.md):  
During projection rebuild via `EventReplayService.ReplayAsync`, the `ExecutionHash` and
`PolicyDecisionHash` fields will carry the literal string `"replay"`. This is by design
and must NOT be treated as a validation error. The EventMetadataAggregate invariants
explicitly tolerate this sentinel.

## Dependencies
- `core-system/identifier` — deterministic ID generation via IIdGenerator
- `core-system/temporal` — IssuedAt via IClock
