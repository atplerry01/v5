# Domain: Match

## Classification

business-system

## Context

marketplace

## Boundary Statement

This domain defines marketplace match pairing structure. No matching algorithms, scoring, or execution logic.

## Lifecycle Pattern

**TERMINAL** — Matched. Immutable once created. No state transitions after creation.

## Purpose

Defines a marketplace match — an immutable pairing between two compatible sides of a transaction.

## Core Responsibilities

* Define match identity and side references
* Enforce that both sides are valid and distinct
* Ensure immutability after creation
* Ensure no implicit state changes

## Aggregate(s)

* MatchAggregate
  * Manages the creation and integrity of a marketplace match

## Value Objects

* MatchId — Unique identifier for a match instance
* MatchStatus — Enum for lifecycle state (Matched — single terminal state)
* MatchSideReference — Reference to one side of the match pairing

## Domain Events

* MatchCreatedEvent — Raised when a match is created (terminal, no further events)

## Specifications

* HasValidReferencesSpecification — Both sides must be non-default and distinct
* IsMatchedSpecification — Checks if match is in Matched state

## Domain Services

* MatchService — Domain operations for match management

## Errors

* MissingId — MatchId is required
* MissingSideA — Side A reference is required
* MissingSideB — Side B reference is required
* SidesCannotBeEqual — Match sides must reference different parties

## Invariants

* MatchId must not be null/default
* SideA must not be null/default (must have valid reference)
* SideB must not be null/default (must have valid reference)
* SideA and SideB must be distinct (no self-matching)
* MatchStatus must be a defined enum value
* Match is immutable after creation (terminal lifecycle)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* listing (matches reference listed items)
* offer (matches may pair accepted offers)
* order (matches may produce orders)

## Status

**S4 — Invariants + Specifications Complete**
