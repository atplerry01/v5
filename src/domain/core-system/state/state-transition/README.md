# Domain: StateTransition

## Classification

core-system

## Context

state

## Purpose

Defines the foundational structure for state transitions — the valid movements between states in a lifecycle. Provides reusable transition primitives for state machine definitions.

## Boundary

This domain defines state transition structure only and contains no state machine execution logic.

## Core Responsibilities

* Define the structural contract for transition lifecycle and identity
* Provide primitives for declaring valid state movements within a lifecycle
* Enforce deterministic transition rules for state machine definitions across systems

## Aggregate(s)

* StateTransitionAggregate

  * Inherits canonical `AggregateRoot`; created via `Define(StateTransitionId, TransitionRule)` factory.
  * Transitions: `Activate()`, `Retire()`.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via inherited `Version` property.

## Value Objects

* StateTransitionId — Validated Guid identifier for a state transition instance
* TransitionStatus — Enum: Defined, Active, Retired
* TransitionRule — Record struct with FromState, ToState, TransitionName (all validated non-empty)

## Domain Events

* StateTransitionDefinedEvent(StateTransitionId, TransitionRule) — Raised when a new state transition is defined
* StateTransitionActivatedEvent(StateTransitionId) — Raised when a state transition is activated
* StateTransitionRetiredEvent(StateTransitionId) — Raised when a state transition is retired

## Specifications

* CanActivateSpecification — Status must be Defined
* CanRetireSpecification — Status must be Active

## Errors

* MissingId — StateTransitionId is required.
* MissingTransitionRule — TransitionRule is required.
* InvalidStateTransition — Guard for illegal status transitions.
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

TERMINAL: Defined -> Active -> Retired

## WHEN-NEEDED folders

* `entity/` — Omitted: this BC has no child entities; state is fully carried by the aggregate and its value objects.
* `service/` — Omitted: no cross-aggregate coordination is required within this BC.

## Notes

Core-system must remain minimal, pure, and reusable.
