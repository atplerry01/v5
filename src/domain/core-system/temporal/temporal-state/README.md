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

  * Represents a temporal state instance and its validity period tracking

## Entities

* None

## Value Objects

* TemporalStateId — Unique identifier for a temporal-state instance

## Domain Events

* TemporalStateCreatedEvent — Raised when a new temporal-state is created
* TemporalStateUpdatedEvent — Raised when temporal-state metadata is updated
* TemporalStateStateChangedEvent — Raised when temporal-state lifecycle state transitions

## Specifications

* TemporalStateSpecification — Validates temporal-state structure and completeness

## Domain Services

* TemporalStateService — Domain operations for temporal-state management

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Created → Active → Updated → Deprecated

## Notes

Core-system must remain minimal, pure, and reusable.
