# Domain: TimeWindow

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational structure for time windows — bounded temporal ranges used for windowed operations, aggregations, and validity periods. Provides reusable time window primitives.

## Core Responsibilities

* Define bounded temporal ranges for windowed operations and aggregations
* Provide reusable primitives for validity period calculations and overlap detection
* Support consistent time window arithmetic across all system components

## Aggregate(s)

* TimeWindowAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* TimeWindowId — Unique identifier for a time-window instance

## Domain Events

* TimeWindowCreatedEvent — Raised when a new time-window is created

## Specifications

* TimeWindowSpecification — Reserved for future structural validation

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
