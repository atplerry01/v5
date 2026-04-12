# Domain: GeoIndex

## Classification

intelligence-system

## Context

geo

## Purpose

Defines the structure of geographic index records — spatial indices for efficient geographic lookups.

## Core Responsibilities

* Define geo index structure and metadata
* Enforce geo index invariants and validation rules
* Emit domain events on geo index lifecycle transitions

## Aggregate(s)

* GeoIndexAggregate

  * Root aggregate representing a geographic index with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* GeoIndexId — Unique identifier for a geo index instance

## Domain Events

* GeoIndexCreatedEvent — Raised when a new geo index is created
* GeoIndexUpdatedEvent — Raised when geo index metadata is updated
* GeoIndexStateChangedEvent — Raised when geo index lifecycle state transitions

## Specifications

* GeoIndexSpecification — Validates geo index structure and completeness

## Domain Services

* GeoIndexService — Domain operations for geo index management

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
