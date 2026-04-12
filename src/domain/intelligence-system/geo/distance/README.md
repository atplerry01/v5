# Domain: Distance

## Classification

intelligence-system

## Context

geo

## Purpose

Defines the structure of geographic distance records — the measured or calculated distances between geographic points.

## Core Responsibilities

* Define distance structure and metadata
* Enforce distance invariants and validation rules
* Emit domain events on distance lifecycle transitions

## Aggregate(s)

* DistanceAggregate

  * Root aggregate representing a geographic distance record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* DistanceId — Unique identifier for a distance instance

## Domain Events

* DistanceCreatedEvent — Raised when a new distance is created
* DistanceUpdatedEvent — Raised when distance metadata is updated
* DistanceStateChangedEvent — Raised when distance lifecycle state transitions

## Specifications

* DistanceSpecification — Validates distance structure and completeness

## Domain Services

* DistanceService — Domain operations for distance management

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
