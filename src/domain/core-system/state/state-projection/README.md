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

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* StateProjectionId — Unique identifier for a state projection instance

## Domain Events

* StateProjectionCreatedEvent — Raised when a new state projection is created

## Specifications

* StateProjectionSpecification — Reserved for future structural validation

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
