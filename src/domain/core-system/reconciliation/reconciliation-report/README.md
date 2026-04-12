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

## Entities

* None

## Value Objects

* ReconciliationReportId — Unique identifier for a reconciliation report instance

## Domain Events

* ReconciliationReportCreatedEvent — Raised when a new reconciliation report is created
* ReconciliationReportUpdatedEvent — Raised when reconciliation report metadata is updated
* ReconciliationReportStateChangedEvent — Raised when reconciliation report lifecycle state transitions

## Specifications

* ReconciliationReportSpecification — Validates reconciliation report structure and completeness

## Domain Services

* ReconciliationReportService — Domain operations for reconciliation report management

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
