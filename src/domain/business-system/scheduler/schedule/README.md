# Domain: Schedule

## Classification

business-system

## Context

scheduler

## Purpose

Defines the structure of schedules — the arrangement of slots into a planned timetable. A schedule organizes slots and must not contain conflicting slots. No scheduling execution logic.

## Core Responsibilities

* Define the structural representation of schedules
* Enforce slot organization and consistency rules
* Support reversible lifecycle (suspend/reactivate)
* Emit domain events on schedule lifecycle transitions

## Aggregate(s)

* ScheduleAggregate
  * Represents the root entity for a schedule, encapsulating identity and lifecycle rules

## Entities

* None

## Value Objects

* ScheduleId — Unique identifier for a schedule instance
* ScheduleStatus — Lifecycle state (Active, Suspended, Deactivated)

## Domain Events

* ScheduleCreatedEvent — Raised when a new schedule is created
* ScheduleSuspendedEvent — Raised when a schedule is suspended
* ScheduleReactivatedEvent — Raised when a suspended schedule is reactivated
* ScheduleDeactivatedEvent — Raised when a schedule is permanently deactivated
* ScheduleUpdatedEvent — Raised when schedule metadata is updated
* ScheduleStateChangedEvent — Raised when schedule lifecycle state transitions

## Specifications

* CanSuspendScheduleSpecification — Guards transition from Active to Suspended
* CanReactivateScheduleSpecification — Guards transition from Suspended to Active
* CanDeactivateScheduleSpecification — Guards transition to Deactivated
* ScheduleSpecification — Validates schedule structure and completeness

## Domain Services

* ScheduleService — Coordination placeholder for schedule domain operations

## Invariants

* Schedule must have a valid identity (non-empty ScheduleId)
* Must organize slots without conflicts
* Must not contain conflicting slots (enforced at service/policy level)
* Status must be a defined enum value

## Boundary Statement

This domain owns schedule definition only. No scheduling execution, no background jobs, no cron logic, no automatic triggers. Slot conflict detection is enforced at the service/policy level.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* slot (schedule organizes slots)
* calendar (schedules grouped into calendars)
* availability (schedule respects availability windows)

## Lifecycle

Active → Suspended → Active (reversible)
Active | Suspended → Deactivated (terminal)

**Pattern: REVERSIBLE** — Can be suspended and reactivated. Deactivation is terminal.

## Status

**S4 — Invariants + Specifications Complete**
