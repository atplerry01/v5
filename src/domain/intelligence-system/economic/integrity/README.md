# Domain: Integrity

## Classification

intelligence-system

## Context

economic

## Purpose

Defines the structure of economic integrity records — the validation state of economic data consistency.

## Core Responsibilities

* Define integrity structure and metadata
* Track lifecycle state of integrity records
* Enforce structural invariants for integrity validity

## Aggregate(s)

* IntegrityAggregate

  * Manages the lifecycle and invariants of an integrity record

## Entities

* None

## Value Objects

* IntegrityId — Unique identifier for an integrity instance

## Domain Events

* IntegrityCreatedEvent — Raised when a new integrity is created
* IntegrityUpdatedEvent — Raised when integrity metadata is updated
* IntegrityStateChangedEvent — Raised when integrity lifecycle state transitions

## Specifications

* IntegritySpecification — Validates integrity structure and completeness

## Domain Services

* IntegrityService — Domain operations for integrity management

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
