# Domain: Diagnostic

## Classification

intelligence-system

## Context

observability

## Purpose

Defines the structure of diagnostic records — the structured analysis outputs for system troubleshooting.

## Core Responsibilities

* Define the canonical structure of diagnostic artifacts
* Enforce diagnostic validity and completeness invariants
* Emit domain events on diagnostic lifecycle transitions

## Aggregate(s)

* DiagnosticAggregate

  * Root aggregate representing a diagnostic instance and its lifecycle

## Entities

* None

## Value Objects

* DiagnosticId — Unique identifier for a diagnostic instance

## Domain Events

* DiagnosticCreatedEvent — Raised when a new diagnostic is created
* DiagnosticUpdatedEvent — Raised when diagnostic metadata is updated
* DiagnosticStateChangedEvent — Raised when diagnostic lifecycle state transitions

## Specifications

* DiagnosticSpecification — Validates diagnostic structure and completeness

## Domain Services

* DiagnosticService — Domain operations for diagnostic management

## Invariants

* Intelligence artifacts must be deterministic and traceable
* No execution logic allowed
* No inference logic allowed

## Policy Dependencies

* Governance or usage constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system (consumes insights)
* trust-system (signals influence trust)
* economic-system (signals influence risk)

## Lifecycle

Created → Updated → Evaluated → Archived

## Notes

This domain represents intelligence structure ONLY. All AI/ML execution is external (T3I layer).
