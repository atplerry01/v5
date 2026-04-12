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

  * Manages the lifecycle and integrity of a single system state instance

## Entities

* None

## Value Objects

* SystemStateId — Unique identifier for a system state instance

## Domain Events

* SystemStateCreatedEvent — Raised when a new system state is created
* SystemStateUpdatedEvent — Raised when system state metadata is updated
* SystemStateStateChangedEvent — Raised when system state lifecycle state transitions

## Specifications

* SystemStateSpecification — Validates system state structure and completeness

## Domain Services

* SystemStateService — Domain operations for system state management

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
