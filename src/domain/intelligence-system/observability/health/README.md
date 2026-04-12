# Domain: Health

## Classification

intelligence-system

## Context

observability

## Purpose

Defines the structure of health records — the system health status snapshots and trend data.

## Core Responsibilities

* Define the canonical structure of health artifacts
* Enforce health record validity and completeness invariants
* Emit domain events on health lifecycle transitions

## Aggregate(s)

* HealthAggregate

  * Root aggregate representing a health record instance and its lifecycle

## Entities

* None

## Value Objects

* HealthId — Unique identifier for a health instance

## Domain Events

* HealthCreatedEvent — Raised when a new health is created
* HealthUpdatedEvent — Raised when health metadata is updated
* HealthStateChangedEvent — Raised when health lifecycle state transitions

## Specifications

* HealthSpecification — Validates health structure and completeness

## Domain Services

* HealthService — Domain operations for health management

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
