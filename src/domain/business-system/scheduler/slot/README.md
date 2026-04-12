# Domain: Slot

## Classification

business-system

## Context

scheduler

## Purpose

Defines the structure of time slots — atomic time blocks with defined start and end. A slot must have valid duration (> 0). No time execution, no automatic triggers.

## Core Responsibilities

* Define the structural representation of time slots
* Enforce valid time slot duration (end after start)
* Emit domain events on slot lifecycle transitions

## Aggregate(s)

* SlotAggregate
  * Represents the root entity for a time slot, encapsulating its time boundaries and lifecycle rules

## Entities

* None

## Value Objects

* SlotId — Unique identifier for a slot instance
* TimeSlot — Start/end ticks defining the slot boundaries (end > start)
* SlotStatus — Lifecycle state (Open, Cancelled)

## Domain Events

* SlotCreatedEvent — Raised when a new slot is created
* SlotCancelledEvent — Raised when a slot is cancelled
* SlotUpdatedEvent — Raised when slot metadata is updated
* SlotStateChangedEvent — Raised when slot lifecycle state transitions

## Specifications

* CanCancelSlotSpecification — Guards transition from Open to Cancelled
* SlotSpecification — Validates slot structure and completeness

## Domain Services

* SlotService — Coordination placeholder for slot domain operations

## Invariants

* Slot must have a valid identity (non-empty SlotId)
* Must define start and end (end after start)
* Duration must be valid (> 0)
* Status must be a defined enum value

## Boundary Statement

This domain owns time slot definition only. No scheduling execution, no background jobs, no timers, no automatic triggers.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* availability (slots allocated within availability windows)
* schedule (slots organized into schedules)

## Lifecycle

Open → Cancelled

**Pattern: TERMINAL** — Once cancelled, slot cannot be reopened.

## Status

**S4 — Invariants + Specifications Complete**
