# Domain: Recurrence

## Classification

business-system

## Context

scheduler

## Purpose

Defines repeating time patterns — the rules that describe how a scheduled event recurs over time (daily, weekly, monthly, yearly) within defined bounds.

## Boundary Statement

This domain defines time coordination contracts only and contains no execution logic.

## Core Responsibilities

* Define and maintain recurrence pattern structure
* Enforce valid pattern rules (frequency, interval, bounds)
* Ensure immutability after creation (pattern is locked)
* Support terminal lifecycle (active to terminated)
* Emit domain events on recurrence lifecycle transitions

## Aggregate(s)

* RecurrenceAggregate
  * Represents the root entity for a recurrence rule, encapsulating pattern definition and lifecycle

## Entities

* None

## Value Objects

* RecurrenceId — Unique identifier for a recurrence instance
* RecurrencePattern — Defines frequency, interval, start, and bounds (end ticks or max occurrences)
* RecurrenceFrequency — Pattern type (Daily, Weekly, Monthly, Yearly)
* RecurrenceStatus — Lifecycle state (Active, Terminated)

## Domain Events

* RecurrenceCreatedEvent — Raised when a new recurrence pattern is defined
* RecurrenceTerminatedEvent — Raised when a recurrence is permanently terminated

## Specifications

* CanTerminateRecurrenceSpecification — Guards transition from Active to Terminated
* ValidRecurrencePatternSpecification — Validates pattern frequency, interval, and bounds
* RecurrenceSpecification — Validates recurrence structure and completeness

## Domain Services

* RecurrenceService — Coordination placeholder for recurrence domain operations

## Invariants

* Recurrence must have a valid identity (non-empty RecurrenceId)
* Must define a valid pattern (defined frequency, positive interval)
* Must define bounds (either end ticks or max occurrences)
* Pattern is immutable after creation
* Must NOT generate occurrences (no execution)
* Status must be a defined enum value

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* schedule (recurrence informs schedule repetition)
* slot (recurrence defines repeating slot patterns)

## Lifecycle

Active -> Terminated (terminal)

**Pattern: TERMINAL** — Active until terminated. No reversibility. Immutable after creation.

## Status

**S4 — Invariants + Specifications Complete**
