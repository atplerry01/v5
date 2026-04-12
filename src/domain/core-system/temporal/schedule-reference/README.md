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

  * Represents a schedule reference instance and its coordination state

## Entities

* None

## Value Objects

* ScheduleReferenceId — Unique identifier for a schedule-reference instance

## Domain Events

* ScheduleReferenceCreatedEvent — Raised when a new schedule-reference is created
* ScheduleReferenceUpdatedEvent — Raised when schedule-reference metadata is updated
* ScheduleReferenceStateChangedEvent — Raised when schedule-reference lifecycle state transitions

## Specifications

* ScheduleReferenceSpecification — Validates schedule-reference structure and completeness

## Domain Services

* ScheduleReferenceService — Domain operations for schedule-reference management

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
