# Domain: ReconciliationReport

## Classification

core-system

## Context

reconciliation

## Purpose

Defines the foundational structure for reconciliation reports — the summary output of a reconciliation run. Provides reusable report primitives for reconciliation result documentation.

## Core Responsibilities

* Represent the summary output of a completed reconciliation run
* Provide structured report primitives for documenting reconciliation results
* Track report generation lifecycle and metadata

## Aggregate(s)

* ReconciliationReportAggregate

  * Manages the lifecycle and state of a reconciliation report instance

## Value Objects

* ReconciliationReportId — Unique identifier for a reconciliation report instance

## Domain Events

* ReconciliationReportCreatedEvent — Raised when a new reconciliation report is created

## Specifications

* ReconciliationReportSpecification — Validates reconciliation report structure and completeness

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
