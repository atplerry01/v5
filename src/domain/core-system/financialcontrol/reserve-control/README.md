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

## Value Objects

* ReserveControlId — Unique identifier for a reserve-control instance

## Domain Events

* ReserveControlCreatedEvent — Raised when a new reserve-control is created

## Specifications

* ReserveControlSpecification — Validates reserve-control structure and completeness

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

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Notes

Core-system must remain minimal, pure, and reusable.
