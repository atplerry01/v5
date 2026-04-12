# Domain: Bid

## Classification: business-system
## Context: marketplace
## Status: S4 — Invariants + Specifications Complete

## Purpose

Defines the structure of a competitive offer in a marketplace. A bid represents a participant's formal submission to acquire goods or services against a listing or market context.

## Boundary Statement

This domain defines bid **structure only**. No pricing logic, bidding algorithms, financial transfers, or external system interaction is permitted. Bid evaluation and acceptance are handled by other domains.

## Aggregate(s)

* BidAggregate — Event-sourced aggregate managing bid lifecycle and invariants

## Value Objects

* BidId — Unique identifier for a bid instance (validated non-empty)
* BidStatus — Lifecycle state enum (Draft, Placed, Withdrawn)
* BidReference — Typed reference to a listing or market (target ID + type)

## Domain Events

* BidCreatedEvent(BidId, BidReference) — Raised when a new bid is created
* BidPlacedEvent(BidId) — Raised when a bid is formally placed
* BidWithdrawnEvent(BidId) — Raised when a placed bid is withdrawn

## Specifications

* CanPlaceSpecification — Validates bid can transition from Draft to Placed
* CanWithdrawSpecification — Validates bid can transition from Placed to Withdrawn

## Domain Services

* BidService — Reserved for domain operations

## Errors

* BidErrors.MissingId — BidId is required
* BidErrors.MissingReference — Bid must reference a listing or market
* BidErrors.InvalidStateTransition — Illegal lifecycle transition attempted

## Invariants

* Bid must have a valid, non-empty BidId at all times
* Bid must reference a listing or market (BidReference) — cannot exist without context
* Bid must define bid terms via its reference
* Status must be a defined BidStatus value
* No financial or execution logic allowed

## Lifecycle Pattern: REVERSIBLE

```
Draft → Placed → Withdrawn
```

* Draft: initial state after creation
* Placed: bid formally submitted
* Withdrawn: bid retracted (reversible — structural withdrawal, not deletion)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
