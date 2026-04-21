# Domain: Timeline

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational structure for timelines — ordered sequences of temporal events and milestones. Provides reusable timeline primitives for tracking temporal progression.

## Core Responsibilities

* Model ordered sequences of temporal events and milestones
* Provide reusable primitives for tracking temporal progression and history
* Support consistent timeline traversal and milestone resolution across systems

## Aggregate(s)

* TimelineAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* TimelineId — Unique identifier for a timeline instance

## Domain Events

* TimelineCreatedEvent — Raised when a new timeline is created

## Specifications

* TimelineSpecification — Reserved for future structural validation

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
