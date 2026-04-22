# platform-system / event / event-envelope

**Classification:** platform-system  
**Context:** event  
**Domain:** event-envelope

## Purpose
Canonical envelope wrapping a domain event payload. The envelope carries all addressing and causation metadata; the payload is opaque to this domain.

## Scope
- Envelope value object: eventId, type, source DomainRoute, causationId, correlationId, occurredAt, payload (opaque bytes)
- Envelope validation: required fields present, source valid DomainRoute, occurredAt set

## Does Not Own
- Payload interpretation (→ engine / projections)
- Subscription or routing (→ event-stream, routing context)
- Authorization (→ control-system/system-policy)

## Inputs
- Event type, source DomainRoute, causation IDs, occurredAt, payload

## Outputs
- Validated `EventEnvelope` value object (no events — pure structural)

## Invariants
- eventId is deterministic: SHA256 of (type + source + causationId + occurredAt)
- source is a valid DomainRoute three-tuple
- occurredAt is set by the caller (via IClock)

## Dependencies
- `core-system/identifier` — event and causation IDs
- `core-system/temporal` — occurredAt timestamp type
