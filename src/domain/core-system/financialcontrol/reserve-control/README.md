# Domain: ReserveControl

## Classification

core-system

## Context

financialcontrol

## Purpose

Defines the foundational structure for reserve tracking and control boundaries. Provides reusable reserve primitives for fund reservation and release patterns.

## Core Responsibilities

* Define reserve tracking structure and control boundaries
* Track fund reservation and release state transitions
* Provide reusable reserve primitives for cross-system reservation patterns

## Aggregate(s)

* ReserveControlAggregate

  * Manages the lifecycle and state of a reserve control instance

## Entities

* None

## Value Objects

* ReserveControlId — Unique identifier for a reserve-control instance

## Domain Events

* ReserveControlCreatedEvent — Raised when a new reserve-control is created
* ReserveControlUpdatedEvent — Raised when reserve-control metadata is updated
* ReserveControlStateChangedEvent — Raised when reserve-control lifecycle state transitions

## Specifications

* ReserveControlSpecification — Validates reserve-control structure and completeness

## Domain Services

* ReserveControlService — Domain operations for reserve-control management

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
