# Domain: Query

## Classification

intelligence-system

## Context

search

## Purpose

Defines the structure of search query records — the structured representations of search requests and their parameters.

## Core Responsibilities

* Model search query structures and parameters
* Track query configuration and filter criteria
* Maintain query lifecycle and state transitions

## Aggregate(s)

* QueryAggregate

  * Encapsulates the lifecycle and invariants of a search query record

## Entities

* None

## Value Objects

* QueryId — Unique identifier for a query instance

## Domain Events

* QueryCreatedEvent — Raised when a new query is created
* QueryUpdatedEvent — Raised when query metadata is updated
* QueryStateChangedEvent — Raised when query lifecycle state transitions

## Specifications

* QuerySpecification — Validates query structure and completeness

## Domain Services

* QueryService — Domain operations for query management

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
