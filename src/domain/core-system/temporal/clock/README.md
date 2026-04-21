# Domain: Clock

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational clock abstraction — the system's canonical time source. Provides reusable clock primitives for deterministic time access across all systems (via IClock).

## Core Responsibilities

* Provide a canonical, deterministic time source for all system components
* Abstract system clock access behind a testable, injectable interface (IClock)
* Ensure consistent time resolution and formatting across all bounded contexts

## Aggregate(s)

* ClockAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* ClockId — Unique identifier for a clock instance

## Domain Events

* ClockCreatedEvent — Raised when a new clock is created

## Specifications

* ClockSpecification — Reserved for future structural validation

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
