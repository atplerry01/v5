# platform-system / command / command-envelope

**Classification:** platform-system  
**Context:** command  
**Domain:** command-envelope

## Purpose
Canonical envelope wrapping a command payload. The envelope carries all addressing and correlation metadata; the payload is opaque to this domain.

## Scope
- Envelope value object: commandId, type, source DomainRoute, destination DomainRoute, correlationId, causationId, issuedAt, payload (opaque bytes)
- Envelope validation: required fields present, IDs non-empty, source and destination valid

## Does Not Own
- Payload interpretation (→ engine layer)
- Routing logic (→ command-routing)
- Authorization (→ control-system/system-policy)

## Inputs
- Command type, source, destination, correlation IDs, payload

## Outputs
- Validated `CommandEnvelope` value object (no events — pure structural)

## Invariants
- commandId is deterministic: SHA256 of (type + source + correlationId + issuedAt)
- source and destination are valid DomainRoute three-tuples
- issuedAt is set by the caller (via IClock); the domain does not read system time

## Dependencies
- `core-system/identifier` — command and correlation IDs
- `core-system/temporal` — issuedAt timestamp type
