# Domain: Ordering

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational structure for event and operation ordering — the rules governing sequence and precedence. Provides reusable ordering primitives for deterministic sequencing.

## Core Responsibilities

* Establish deterministic sequence rules for events and operations
* Provide ordering primitives that guarantee consistent precedence resolution
* Support causal and temporal ordering across distributed system boundaries

## Aggregate(s)

* OrderingAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* OrderingId — Unique identifier for an ordering instance

## Domain Events

* OrderingCreatedEvent — Raised when a new ordering is created

## Specifications

* OrderingSpecification — Reserved for future structural validation

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
