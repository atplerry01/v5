# Domain: RegionalIndex

## Classification

intelligence-system

## Context

estimation

## Purpose

Defines the structure of regional index records — location-based adjustment indices for estimation calibration.

## Core Responsibilities

* Define regional index structure and metadata
* Enforce regional index invariants and validation rules
* Emit domain events on regional index lifecycle transitions

## Aggregate(s)

* RegionalIndexAggregate

  * Root aggregate representing a regional index with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* RegionalIndexId — Unique identifier for a regional index instance

## Domain Events

* RegionalIndexCreatedEvent — Raised when a new regional index is created
* RegionalIndexUpdatedEvent — Raised when regional index metadata is updated
* RegionalIndexStateChangedEvent — Raised when regional index lifecycle state transitions

## Specifications

* RegionalIndexSpecification — Validates regional index structure and completeness

## Domain Services

* RegionalIndexService — Domain operations for regional index management

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
