# Domain: Reference

## Classification

intelligence-system

## Context

knowledge

## Purpose

Defines the structure of knowledge references — the citations and source pointers for knowledge claims.

## Core Responsibilities

* Define the canonical structure for reference records
* Track lifecycle state of reference entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* ReferenceAggregate

  * Represents a single reference record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ReferenceId — Unique identifier for a reference instance

## Domain Events

* ReferenceCreatedEvent — Raised when a new reference is created
* ReferenceUpdatedEvent — Raised when reference metadata is updated
* ReferenceStateChangedEvent — Raised when reference lifecycle state transitions

## Specifications

* ReferenceSpecification — Validates reference structure and completeness

## Domain Services

* ReferenceService — Domain operations for reference management

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
