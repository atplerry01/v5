# Domain: Ledger

## Classification

constitutional-system

## Context

chain

## Purpose

Defines the constitutional chain ledger structure — the immutable record of all constitutional events and decisions anchored into the chain. The ledger represents the append-only audit trail that proves constitutional compliance over time.

## Core Responsibilities

* Define ledger entry structure and identity
* Define chain anchoring rules
* Define immutability constraints for ledger records

## Aggregate(s)

* LedgerAggregate

  * Represents the constitutional ledger container — the root for chain entries

## Entities

* None

## Value Objects

* LedgerId — Unique identifier for a ledger instance

## Domain Events

* LedgerCreatedEvent — Raised when a new ledger is instantiated
* LedgerUpdatedEvent — Raised when ledger metadata is updated
* LedgerStateChangedEvent — Raised when ledger lifecycle state transitions

## Specifications

* LedgerSpecification — Validates ledger structure and entry integrity

## Domain Services

* LedgerService — Domain operations for ledger management

## Invariants

* Ledger entries must be immutable once anchored
* Ledger must maintain append-only semantics
* All entries must be traceable to a policy decision
* Rules must be policy-bound (WHYCEPOLICY)

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines structure, not enforcement

## Integration Points

* decision-system
* trust-system
* economic-system
* WHYCEPOLICY engine (external)

## Lifecycle

Draft → Defined → Activated → Versioned → Deprecated

## Notes

This domain defines constitutional structure ONLY.
All enforcement is external via WHYCEPOLICY and runtime.