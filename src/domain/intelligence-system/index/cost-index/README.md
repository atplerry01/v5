# Domain: CostIndex

## Classification

intelligence-system

## Context

index

## Purpose

Defines the structure of cost index records — the composite indicators tracking cost trends over time.

## Core Responsibilities

* Define the canonical structure for cost index records
* Track lifecycle state of cost index entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* CostIndexAggregate

  * Represents a single cost index record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* CostIndexId — Unique identifier for a cost-index instance

## Domain Events

* CostIndexCreatedEvent — Raised when a new cost-index is created
* CostIndexUpdatedEvent — Raised when cost-index metadata is updated
* CostIndexStateChangedEvent — Raised when cost-index lifecycle state transitions

## Specifications

* CostIndexSpecification — Validates cost-index structure and completeness

## Domain Services

* CostIndexService — Domain operations for cost-index management

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
