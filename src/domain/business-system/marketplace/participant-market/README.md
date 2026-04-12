# Domain: ParticipantMarket

## Classification: business-system
## Context: marketplace
## Status: S4 — Invariants + Specifications Complete

## Purpose

Defines the structural link between a participant and a marketplace. A participant-market record represents a participant's access to and standing within a specific market.

## Boundary Statement

This domain defines participant-market **access structure only**. No identity resolution, financial operations, or external system interaction is permitted. Participant identity is managed by structural-system; this domain only holds the typed reference.

## Aggregate(s)

* ParticipantMarketAggregate — Event-sourced aggregate managing participant-market lifecycle and access constraints

## Value Objects

* ParticipantMarketId — Unique identifier for a participant-market instance (validated non-empty)
* ParticipantMarketStatus — Lifecycle state enum (Registered, Active, Suspended)
* ParticipantReference — Typed link between a participant ID and a market ID (both validated non-empty)

## Domain Events

* ParticipantMarketRegisteredEvent(ParticipantMarketId, ParticipantReference) — Raised when a participant registers for a market
* ParticipantMarketActivatedEvent(ParticipantMarketId) — Raised when participation is activated
* ParticipantMarketSuspendedEvent(ParticipantMarketId) — Raised when participation is suspended

## Specifications

* CanActivateSpecification — Validates transition from Registered or Suspended to Active
* CanSuspendSpecification — Validates transition from Active to Suspended

## Domain Services

* ParticipantMarketService — Reserved for domain operations

## Errors

* ParticipantMarketErrors.MissingId — ParticipantMarketId is required
* ParticipantMarketErrors.MissingReference — Must link participant to market
* ParticipantMarketErrors.InvalidStateTransition — Illegal lifecycle transition attempted

## Invariants

* Participant-market must have a valid, non-empty ParticipantMarketId at all times
* Must link a participant to a market (ParticipantReference with both IDs non-empty)
* Must enforce access constraints via specification-guarded transitions
* Status must be a defined ParticipantMarketStatus value
* No financial or execution logic allowed

## Lifecycle Pattern: REVERSIBLE

```
Registered → Active ↔ Suspended
```

* Registered: initial state after creation
* Active: participant has active market access
* Suspended: participation temporarily suspended (can reactivate — reversible)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system (participant identity)
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
