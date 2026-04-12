# Domain: Booking

## Classification

business-system

## Context

scheduler

## Purpose

Defines the reservation of time and resource — the confirmed or pending claim on a time range for a specific activity or resource.

## Boundary Statement

This domain defines time coordination contracts only and contains no execution logic.

## Core Responsibilities

* Define and maintain booking record structure
* Enforce valid time range (end after start)
* Validate booking conflicts via specification
* Support reversible lifecycle (confirm/cancel)
* Emit domain events on booking lifecycle transitions

## Aggregate(s)

* BookingAggregate
  * Represents the root entity for a booking, encapsulating time range, status, and lifecycle rules

## Entities

* None

## Value Objects

* BookingId — Unique identifier for a booking instance
* BookingTimeRange — Start/end ticks defining the booked window (end > start), with overlap detection
* BookingStatus — Lifecycle state (Pending, Confirmed, Cancelled)

## Domain Events

* BookingCreatedEvent — Raised when a new booking is created
* BookingConfirmedEvent — Raised when a pending booking is confirmed
* BookingCancelledEvent — Raised when a booking is cancelled

## Specifications

* CanConfirmBookingSpecification — Guards transition from Pending to Confirmed
* CanCancelBookingSpecification — Guards transition from Pending or Confirmed to Cancelled
* NoConflictBookingSpecification — Validates that a proposed booking does not overlap with existing bookings
* BookingSpecification — Validates booking structure and completeness

## Domain Services

* BookingService — Coordination placeholder for booking domain operations

## Invariants

* Booking must have a valid identity (non-empty BookingId)
* Must reference a valid time range (end after start)
* Cannot overlap conflicting bookings (enforced via NoConflictBookingSpecification)
* Must be explicit (no auto-allocation)
* Status must be a defined enum value

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* slot (booking claims a time slot)
* schedule (booking references a schedule)
* availability (booking must fall within available windows)

## Lifecycle

Pending -> Confirmed -> Cancelled (terminal)
Pending -> Cancelled (terminal)

**Pattern: REVERSIBLE** — Can be confirmed and cancelled. Cancellation is terminal.

## Status

**S4 — Invariants + Specifications Complete**
