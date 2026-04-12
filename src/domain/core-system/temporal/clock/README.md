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

  * Represents the canonical clock instance and its configuration state

## Entities

* None

## Value Objects

* ClockId — Unique identifier for a clock instance

## Domain Events

* ClockCreatedEvent — Raised when a new clock is created
* ClockUpdatedEvent — Raised when clock metadata is updated
* ClockStateChangedEvent — Raised when clock lifecycle state transitions

## Specifications

* ClockSpecification — Validates clock structure and completeness

## Domain Services

* ClockService — Domain operations for clock management

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
