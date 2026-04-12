# Domain: ReconciliationRun

## Classification

core-system

## Context

reconciliation

## Purpose

Defines the foundational structure for reconciliation runs — a single execution of a reconciliation process. Provides reusable run primitives for tracking reconciliation execution lifecycle.

## Core Responsibilities

* Represent a single execution instance of a reconciliation process
* Provide structured primitives for tracking reconciliation execution lifecycle
* Manage run-level state transitions from initiation through completion

## Aggregate(s)

* ReconciliationRunAggregate

  * Manages the lifecycle and state of a reconciliation run instance

## Entities

* None

## Value Objects

* ReconciliationRunId — Unique identifier for a reconciliation run instance

## Domain Events

* ReconciliationRunCreatedEvent — Raised when a new reconciliation run is created
* ReconciliationRunUpdatedEvent — Raised when reconciliation run metadata is updated
* ReconciliationRunStateChangedEvent — Raised when reconciliation run lifecycle state transitions

## Specifications

* ReconciliationRunSpecification — Validates reconciliation run structure and completeness

## Domain Services

* ReconciliationRunService — Domain operations for reconciliation run management

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
