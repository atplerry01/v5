# platform-system / event

**Classification:** platform-system  
**Context:** event

## Purpose
Owns the pure structural model of a domain event: its envelope (addressing + metadata), definition (registered event types), schema (canonical payload shape), and stream (ordered sequence of events from a source).

## Domains
| Domain | Responsibility |
|---|---|
| `event-definition` | Registered event type definitions with schema reference and stream binding |
| `event-schema` | Canonical schema structure for an event payload — fields, types, compatibility mode |
| `event-envelope` | The canonical envelope wrapping an event payload: ID, type, aggregateId, causation, timestamp |
| `event-metadata` | Structured metadata on an event: ExecutionHash, PolicyDecisionHash, actor, trace IDs |
| `event-stream` | Ordered sequence declaration for events from a declared source DomainRoute |

## Does Not Own
- Event publishing or subscription (→ engine / runtime)
- Business-specific event payloads (→ consuming domain systems)
- Event processing or projection (→ projections layer)
- Policy evaluation (→ runtime pipeline; event-metadata carries hashes as structural carrier only)

## Inputs
- Event type registration at bootstrap
- Stream registration from engine layer

## Outputs
- `EventDefinitionRegisteredEvent`, `EventDefinitionDeprecatedEvent`
- `EventSchemaRegisteredEvent`, `EventSchemaDeprecatedEvent`
- `EventEnvelopeCreatedEvent`
- `EventMetadataAttachedEvent`
- `EventStreamDeclaredEvent`, `EventStreamArchivedEvent`

## Invariants
- All IDs are deterministic: SHA256-derived via IIdGenerator
- Events are IMMUTABLE once emitted — no state mutation after EventEnvelopeCreatedEvent
- Every event type maps to exactly one published schema
- Stream order is monotonically increasing within a source DomainRoute
- event-metadata carries ExecutionHash and PolicyDecisionHash as structural carriers —
  replay sentinels ("replay") are tolerated per REPLAY-SENTINEL-PROTECTED-01

## Dependencies
- `core-system/identifier` — event and causation IDs
- `core-system/temporal` — event occurredAt timestamp
