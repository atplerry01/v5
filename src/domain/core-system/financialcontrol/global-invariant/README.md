# Domain: GlobalInvariant

## Classification

core-system

## Context

financialcontrol

## Purpose

Defines system-wide financial invariants that must hold across all operations. Provides the foundational consistency rules for financial state integrity.

## Core Responsibilities

* Define system-wide financial invariants and consistency rules
* Track invariant validation state across operations
* Provide foundational consistency primitives for financial state integrity

## Aggregate(s)

* GlobalInvariantAggregate

  * Manages the lifecycle and state of a global invariant instance

## Entities

* None

## Value Objects

* GlobalInvariantId — Unique identifier for a global-invariant instance

## Domain Events

* GlobalInvariantCreatedEvent — Raised when a new global-invariant is created
* GlobalInvariantUpdatedEvent — Raised when global-invariant metadata is updated
* GlobalInvariantStateChangedEvent — Raised when global-invariant lifecycle state transitions

## Specifications

* GlobalInvariantSpecification — Validates global-invariant structure and completeness

## Domain Services

* GlobalInvariantService — Domain operations for global-invariant management

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Created → Active → Updated → Deprecated

## Notes

Core-system must remain minimal, pure, and reusable.
