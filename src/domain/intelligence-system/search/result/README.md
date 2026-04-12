# Domain: Result

## Classification

intelligence-system

## Context

search

## Purpose

Defines the structure of search result records — the matched items returned from search operations.

## Core Responsibilities

* Model search result structures and matched items
* Track result metadata and relevance scores
* Maintain result lifecycle and state transitions

## Aggregate(s)

* ResultAggregate

  * Encapsulates the lifecycle and invariants of a search result record

## Entities

* None

## Value Objects

* ResultId — Unique identifier for a result instance

## Domain Events

* ResultCreatedEvent — Raised when a new result is created
* ResultUpdatedEvent — Raised when result metadata is updated
* ResultStateChangedEvent — Raised when result lifecycle state transitions

## Specifications

* ResultSpecification — Validates result structure and completeness

## Domain Services

* ResultService — Domain operations for result management

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
