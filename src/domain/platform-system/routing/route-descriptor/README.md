# platform-system / routing / route-descriptor

**Classification:** platform-system  
**Context:** routing  
**Domain:** route-descriptor

## Purpose
A resolved routing descriptor: the structural contract for directing a message from a source DomainRoute to a destination DomainRoute via a declared transport.

## Scope
- Route descriptor value object: descriptorId, source, destination, transportHint, priority
- Route validity: source and destination are non-empty DomainRoute three-tuples

## Does Not Own
- Message delivery (→ infrastructure)
- Routing policy decisions (→ control-system/system-policy)

## Inputs
- Source DomainRoute, destination DomainRoute, transport hint (kafka / http / in-process), priority (integer)

## Outputs
- `RouteDescriptorRegisteredEvent`

## Invariants
- Descriptor ID is deterministic: SHA256 of (source + destination + transportHint)
- Source ≠ destination (self-routing is not permitted)
- Transport hint must be a declared platform transport name

## Dependencies
- `core-system/identifier` — descriptor ID
