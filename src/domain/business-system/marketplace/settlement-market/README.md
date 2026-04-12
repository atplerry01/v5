# Domain: SettlementMarket

## Classification: business-system
## Context: marketplace
## Status: S4 — Invariants + Specifications Complete

## Purpose

Defines the structure of a settlement contract within a marketplace. A settlement-market record defines the rules and terms governing how transactions are settled — without executing any settlement.

## Boundary Statement

This domain defines settlement **contract structure only**. No settlement execution, financial transfers, pricing logic, or external system interaction is permitted. Actual settlement processing is handled by economic-system.

## Aggregate(s)

* SettlementMarketAggregate — Event-sourced aggregate managing settlement contract lifecycle and invariants

## Value Objects

* SettlementMarketId — Unique identifier for a settlement-market instance (validated non-empty)
* SettlementMarketStatus — Lifecycle state enum (Draft, Defined, Sealed)
* SettlementTerms — Settlement contract terms with type and currency (both validated non-empty)

## Domain Events

* SettlementMarketCreatedEvent(SettlementMarketId) — Raised when a new settlement contract is created
* SettlementMarketDefinedEvent(SettlementMarketId, SettlementTerms) — Raised when settlement terms are defined
* SettlementMarketSealedEvent(SettlementMarketId) — Raised when settlement contract is sealed

## Specifications

* CanDefineSpecification — Validates transition from Draft to Defined
* CanSealSpecification — Validates transition from Defined to Sealed

## Domain Services

* SettlementMarketService — Reserved for domain operations

## Errors

* SettlementMarketErrors.MissingId — SettlementMarketId is required
* SettlementMarketErrors.MissingTerms — Terms must be defined before sealing
* SettlementMarketErrors.InvalidStateTransition — Illegal lifecycle transition attempted

## Invariants

* Settlement-market must have a valid, non-empty SettlementMarketId at all times
* Must define settlement rules (terms) before the contract can be sealed
* Must NOT execute settlement — structure definition only
* Status must be a defined SettlementMarketStatus value
* No financial or execution logic allowed

## Lifecycle Pattern: TERMINAL

```
Draft → Defined → Sealed
```

* Draft: initial state after creation (no terms yet)
* Defined: settlement terms have been specified
* Sealed: contract finalized and immutable (terminal — no reversal)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic — settlement execution lives there)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
