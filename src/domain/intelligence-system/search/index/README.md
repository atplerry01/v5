# Domain: Index

## Classification

intelligence-system

## Context

search

## Purpose

Defines the structure of search index records — the indexed data structures enabling efficient search operations.

## Core Responsibilities

* Model search index structures and indexed data
* Track index configuration and field mappings
* Maintain index lifecycle and state transitions

## Aggregate(s)

* IndexAggregate

  * Encapsulates the lifecycle and invariants of a search index record

## Entities

* None

## Value Objects

* IndexId — Unique identifier for an index instance

## Domain Events

* IndexCreatedEvent — Raised when a new index is created
* IndexUpdatedEvent — Raised when index metadata is updated
* IndexStateChangedEvent — Raised when index lifecycle state transitions

## Specifications

* IndexSpecification — Validates index structure and completeness

## Domain Services

* IndexService — Domain operations for index management

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
