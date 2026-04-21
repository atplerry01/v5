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

## Value Objects

* ReconciliationRunId — Unique identifier for a reconciliation run instance

## Domain Events

* ReconciliationRunCreatedEvent — Raised when a new reconciliation run is created

## Specifications

* ReconciliationRunSpecification — Validates reconciliation run structure and completeness

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
