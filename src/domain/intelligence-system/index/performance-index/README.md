# Domain: PerformanceIndex

## Classification

intelligence-system

## Context

index

## Purpose

Defines the structure of performance index records — the composite indicators measuring operational performance.

## Core Responsibilities

* Define the canonical structure for performance index records
* Track lifecycle state of performance index entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* PerformanceIndexAggregate

  * Represents a single performance index record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* PerformanceIndexId — Unique identifier for a performance-index instance

## Domain Events

* PerformanceIndexCreatedEvent — Raised when a new performance-index is created
* PerformanceIndexUpdatedEvent — Raised when performance-index metadata is updated
* PerformanceIndexStateChangedEvent — Raised when performance-index lifecycle state transitions

## Specifications

* PerformanceIndexSpecification — Validates performance-index structure and completeness

## Domain Services

* PerformanceIndexService — Domain operations for performance-index management

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
