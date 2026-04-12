# Domain: Calendar

## Classification

business-system

## Context

scheduler

## Purpose

Defines the structure of calendars — the grouping of schedules into a unified time container. A calendar must group schedules and maintain consistency. No time execution, no automatic triggers.

## Core Responsibilities

* Define the structural representation of calendars
* Enforce schedule grouping and consistency
* Emit domain events on calendar lifecycle transitions

## Aggregate(s)

* CalendarAggregate
  * Represents the root entity for a calendar, encapsulating identity and lifecycle rules

## Entities

* None

## Value Objects

* CalendarId — Unique identifier for a calendar instance
* CalendarStatus — Lifecycle state (Active, Archived)

## Domain Events

* CalendarCreatedEvent — Raised when a new calendar is created
* CalendarArchivedEvent — Raised when a calendar is archived
* CalendarUpdatedEvent — Raised when calendar metadata is updated
* CalendarStateChangedEvent — Raised when calendar lifecycle state transitions

## Specifications

* CanArchiveCalendarSpecification — Guards transition from Active to Archived
* CalendarSpecification — Validates calendar structure and completeness

## Domain Services

* CalendarService — Coordination placeholder for calendar domain operations

## Invariants

* Calendar must have a valid identity (non-empty CalendarId)
* Must group schedules and maintain consistency
* Status must be a defined enum value

## Boundary Statement

This domain owns calendar definition only. No scheduling execution, no background jobs, no cron logic, no automatic triggers. Schedule consistency within a calendar is enforced at the service/policy level.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* schedule (calendar groups schedules)
* structural-system

## Lifecycle

Active → Archived

**Pattern: TERMINAL** — Once archived, calendar cannot be reactivated.

## Status

**S4 — Invariants + Specifications Complete**
