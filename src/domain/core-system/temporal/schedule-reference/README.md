# Domain: ScheduleReference

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational structure for schedule references — pointers to temporal schedules used across the system. Provides reusable schedule reference primitives for time-based coordination.

## Core Responsibilities

* Define canonical schedule reference pointers for cross-system temporal coordination
* Provide reusable primitives for linking entities to temporal schedules
* Ensure consistent schedule reference resolution across bounded contexts

## Aggregate(s)

* ScheduleReferenceAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* ScheduleReferenceId — Unique identifier for a schedule-reference instance

## Domain Events

* ScheduleReferenceCreatedEvent — Raised when a new schedule-reference is created

## Specifications

* ScheduleReferenceSpecification — Reserved for future structural validation

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
