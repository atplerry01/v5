# Domain: Taxonomy

## Classification

intelligence-system

## Context

knowledge

## Purpose

Defines the structure of knowledge taxonomies — the classification hierarchies organizing knowledge domains.

## Core Responsibilities

* Define the canonical structure for taxonomy records
* Track lifecycle state of taxonomy entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* TaxonomyAggregate

  * Represents a single taxonomy record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* TaxonomyId — Unique identifier for a taxonomy instance

## Domain Events

* TaxonomyCreatedEvent — Raised when a new taxonomy is created
* TaxonomyUpdatedEvent — Raised when taxonomy metadata is updated
* TaxonomyStateChangedEvent — Raised when taxonomy lifecycle state transitions

## Specifications

* TaxonomySpecification — Validates taxonomy structure and completeness

## Domain Services

* TaxonomyService — Domain operations for taxonomy management

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
