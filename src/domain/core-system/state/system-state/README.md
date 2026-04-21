# Domain: SystemState

## Classification

core-system

## Context

state

## Purpose

Defines the foundational structure for system-level state — the overall health, mode, and operational status of the system. Provides reusable system state primitives for cross-system status awareness.

## Core Responsibilities

* Define the structural contract for system state lifecycle and identity
* Provide primitives for representing system health, mode, and operational status
* Enforce deterministic system state rules for cross-system status awareness

## Aggregate(s)

* SystemStateAggregate

  * Inherits canonical `AggregateRoot`; skeleton factory `Create()` with `AlreadyInitialized()` guard.

## Value Objects

* SystemStateId — Unique identifier for a system state instance

## Domain Events

* SystemStateCreatedEvent — Raised when a new system state is created

## Specifications

* SystemStateSpecification — Reserved for future structural validation

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
