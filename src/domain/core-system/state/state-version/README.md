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

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* StateVersionId — Unique identifier for a state version instance

## Domain Events

* StateVersionCreatedEvent — Raised when a new state version is created

## Specifications

* StateVersionSpecification — Reserved for future structural validation

## Errors

* AlreadyInitialized — Factory invoked on an already-initialized aggregate.

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Created (skeleton — lifecycle transitions reserved for future expansion)

## WHEN-NEEDED folders

* `entity/` — Omitted: this BC has no child entities; state is fully carried by the aggregate and its value objects.
* `service/` — Omitted: no cross-aggregate coordination is required within this BC.

## Notes

Core-system must remain minimal, pure, and reusable.
