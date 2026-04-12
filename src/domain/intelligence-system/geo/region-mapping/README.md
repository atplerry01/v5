# Domain: RegionMapping

## Classification

intelligence-system

## Context

geo

## Purpose

Defines the structure of region mapping records — the associations between geographic areas and business regions.

## Core Responsibilities

* Define region mapping structure and metadata
* Enforce region mapping invariants and validation rules
* Emit domain events on region mapping lifecycle transitions

## Aggregate(s)

* RegionMappingAggregate

  * Root aggregate representing a region mapping with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* RegionMappingId — Unique identifier for a region mapping instance

## Domain Events

* RegionMappingCreatedEvent — Raised when a new region mapping is created
* RegionMappingUpdatedEvent — Raised when region mapping metadata is updated
* RegionMappingStateChangedEvent — Raised when region mapping lifecycle state transitions

## Specifications

* RegionMappingSpecification — Validates region mapping structure and completeness

## Domain Services

* RegionMappingService — Domain operations for region mapping management

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
