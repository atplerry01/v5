# Domain: StateVersion

## Classification

core-system

## Context

state

## Purpose

Defines the foundational structure for state versioning — tracking the version history of aggregate state changes. Provides reusable version primitives for optimistic concurrency and history tracking.

## Core Responsibilities

* Define the structural contract for version lifecycle and identity
* Provide primitives for tracking aggregate state change history
* Enforce deterministic versioning rules for optimistic concurrency across systems

## Aggregate(s)

* StateVersionAggregate

  * Manages the lifecycle and integrity of a single state version instance

## Entities

* None

## Value Objects

* StateVersionId — Unique identifier for a state version instance

## Domain Events

* StateVersionCreatedEvent — Raised when a new state version is created
* StateVersionUpdatedEvent — Raised when state version metadata is updated
* StateVersionStateChangedEvent — Raised when state version lifecycle state transitions

## Specifications

* StateVersionSpecification — Validates state version structure and completeness

## Domain Services

* StateVersionService — Domain operations for state version management

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
