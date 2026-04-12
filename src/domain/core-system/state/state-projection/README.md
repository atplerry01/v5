# Domain: StateProjection

## Classification

core-system

## Context

state

## Purpose

Defines the foundational structure for state projections — derived read models built from event streams. Provides reusable projection primitives for cross-system state materialization.

## Core Responsibilities

* Define the structural contract for projection lifecycle and identity
* Provide primitives for materializing read models from event streams
* Enforce deterministic projection rules across all consuming systems

## Aggregate(s)

* StateProjectionAggregate

  * Manages the lifecycle and integrity of a single state projection instance

## Entities

* None

## Value Objects

* StateProjectionId — Unique identifier for a state projection instance

## Domain Events

* StateProjectionCreatedEvent — Raised when a new state projection is created
* StateProjectionUpdatedEvent — Raised when state projection metadata is updated
* StateProjectionStateChangedEvent — Raised when state projection lifecycle state transitions

## Specifications

* StateProjectionSpecification — Validates state projection structure and completeness

## Domain Services

* StateProjectionService — Domain operations for state projection management

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
