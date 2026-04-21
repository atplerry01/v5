# Domain: TemporalState

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational structure for temporal state — time-bound state that tracks validity periods and temporal context. Provides reusable temporal state primitives for bi-temporal modeling.

## Core Responsibilities

* Track validity periods and temporal context for time-bound state
* Provide primitives for bi-temporal modeling (transaction time and valid time)
* Ensure consistent temporal state transitions and period management

## Aggregate(s)

* TemporalStateAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* TemporalStateId — Unique identifier for a temporal-state instance

## Domain Events

* TemporalStateCreatedEvent — Raised when a new temporal-state is created

## Specifications

* TemporalStateSpecification — Reserved for future structural validation

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
