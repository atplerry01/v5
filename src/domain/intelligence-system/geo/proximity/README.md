# Domain: Proximity

## Classification

intelligence-system

## Context

geo

## Purpose

Defines the structure of proximity records — the nearness relationships between geographic entities.

## Core Responsibilities

* Define proximity structure and metadata
* Enforce proximity invariants and validation rules
* Emit domain events on proximity lifecycle transitions

## Aggregate(s)

* ProximityAggregate

  * Root aggregate representing a proximity record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ProximityId — Unique identifier for a proximity instance

## Domain Events

* ProximityCreatedEvent — Raised when a new proximity is created
* ProximityUpdatedEvent — Raised when proximity metadata is updated
* ProximityStateChangedEvent — Raised when proximity lifecycle state transitions

## Specifications

* ProximitySpecification — Validates proximity structure and completeness

## Domain Services

* ProximityService — Domain operations for proximity management

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
