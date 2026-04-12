# Domain: ReconciliationException

## Classification

core-system

## Context

reconciliation

## Purpose

Defines the foundational structure for reconciliation exceptions — discrepancies or mismatches detected during reconciliation. Provides reusable exception primitives for cross-system consistency checks.

## Core Responsibilities

* Represent discrepancies detected during reconciliation processes
* Classify and categorize exception types for structured resolution
* Track exception lifecycle from detection through resolution

## Aggregate(s)

* ReconciliationExceptionAggregate

  * Manages the lifecycle and state of a reconciliation exception instance

## Entities

* None

## Value Objects

* ReconciliationExceptionId — Unique identifier for a reconciliation exception instance

## Domain Events

* ReconciliationExceptionCreatedEvent — Raised when a new reconciliation exception is created
* ReconciliationExceptionUpdatedEvent — Raised when reconciliation exception metadata is updated
* ReconciliationExceptionStateChangedEvent — Raised when reconciliation exception lifecycle state transitions

## Specifications

* ReconciliationExceptionSpecification — Validates reconciliation exception structure and completeness

## Domain Services

* ReconciliationExceptionService — Domain operations for reconciliation exception management

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
