# Domain: Utilization

## Classification

intelligence-system

## Context

capacity

## Purpose

Defines the structure of capacity utilization records — the measured usage of available capacity.

## Core Responsibilities

* Define utilization structure and metadata
* Track lifecycle state of utilization records
* Enforce structural invariants for utilization validity

## Aggregate(s)

* UtilizationAggregate

  * Manages the lifecycle and invariants of a utilization record

## Entities

* None

## Value Objects

* UtilizationId — Unique identifier for a utilization instance

## Domain Events

* UtilizationCreatedEvent — Raised when a new utilization is created
* UtilizationUpdatedEvent — Raised when utilization metadata is updated
* UtilizationStateChangedEvent — Raised when utilization lifecycle state transitions

## Specifications

* UtilizationSpecification — Validates utilization structure and completeness

## Domain Services

* UtilizationService — Domain operations for utilization management

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
