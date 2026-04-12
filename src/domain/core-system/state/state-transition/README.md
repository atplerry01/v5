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

  * Factory: Define(id, rule) — creates a new transition in Defined status
  * Transitions: Activate(), Retire()
  * Manages the lifecycle and integrity of a single state transition instance

## Entities

* None

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

## Domain Services

* StateTransitionService — Reserved for future domain operations

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

## Notes

Core-system must remain minimal, pure, and reusable.
