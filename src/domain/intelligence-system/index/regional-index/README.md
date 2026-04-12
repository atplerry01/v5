# Domain: RegionalIndex

## Classification

intelligence-system

## Context

index

## Purpose

Defines the structure of regional index records — the composite indicators measuring economic conditions by region.

## Core Responsibilities

* Define the canonical structure for regional index records
* Track lifecycle state of regional index entries
* Emit domain events on creation, update, and state transitions

## Aggregate(s)

* RegionalIndexAggregate

  * Represents a single regional index record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* RegionalIndexId — Unique identifier for a regional-index instance

## Domain Events

* RegionalIndexCreatedEvent — Raised when a new regional-index is created
* RegionalIndexUpdatedEvent — Raised when regional-index metadata is updated
* RegionalIndexStateChangedEvent — Raised when regional-index lifecycle state transitions

## Specifications

* RegionalIndexSpecification — Validates regional-index structure and completeness

## Domain Services

* RegionalIndexService — Domain operations for regional-index management

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
