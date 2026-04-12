# Domain: Availability

## Classification

business-system

## Context

scheduler

## Purpose

Defines when a resource is available — the time ranges during which resources, people, or services can be scheduled. No time-based execution, no automatic triggers.

## Core Responsibilities

* Define the structural representation of availability windows
* Enforce valid time ranges (end after start)
* Support reversible lifecycle (suspend/reactivate)
* Emit domain events on availability lifecycle transitions

## Aggregate(s)

* AvailabilityAggregate
  * Represents the root entity for an availability window, encapsulating time range and lifecycle rules

## Entities

* None

## Value Objects

* AvailabilityId — Unique identifier for an availability instance
* TimeRange — Start/end ticks defining the availability window (end > start)
* AvailabilityStatus — Lifecycle state (Active, Suspended, Deactivated)

## Domain Events

* AvailabilityCreatedEvent — Raised when a new availability is created
* AvailabilitySuspendedEvent — Raised when availability is suspended
* AvailabilityReactivatedEvent — Raised when a suspended availability is reactivated
* AvailabilityDeactivatedEvent — Raised when availability is permanently deactivated
* AvailabilityUpdatedEvent — Raised when availability metadata is updated
* AvailabilityStateChangedEvent — Raised when availability lifecycle state transitions

## Specifications

* CanSuspendAvailabilitySpecification — Guards transition from Active to Suspended
* CanReactivateAvailabilitySpecification — Guards transition from Suspended to Active
* CanDeactivateAvailabilitySpecification — Guards transition to Deactivated
* AvailabilitySpecification — Validates availability structure and completeness

## Domain Services

* AvailabilityService — Coordination placeholder for availability domain operations

## Invariants

* Availability must have a valid identity (non-empty AvailabilityId)
* Must define valid time range (end after start)
* Cannot overlap invalidly (enforced at service/policy level)
* Status must be a defined enum value

## Boundary Statement

This domain owns availability definition only. No scheduling execution, no background jobs, no cron logic, no automatic triggers.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* slot (availability provides windows for slot allocation)
* schedule (availability informs schedule construction)

## Lifecycle

Active → Suspended → Active (reversible)
Active | Suspended → Deactivated (terminal)

**Pattern: REVERSIBLE** — Can be suspended and reactivated. Deactivation is terminal.

## Status

**S4 — Invariants + Specifications Complete**
