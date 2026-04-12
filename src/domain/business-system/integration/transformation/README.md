# Domain: Transformation

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration transformations — the data conversion rules applied during integration processing.

## Core Responsibilities

* Define the structural identity and metadata of integration transformations
* Track transformation lifecycle states and transitions
* Maintain relationships between transformations and other integration entities

## Aggregate(s)

* Transformation

  * Represents a data conversion rule applied during integration processing

## Entities

* None

## Value Objects

* TransformationId — Unique identifier for a transformation instance

## Domain Events

* TransformationCreatedEvent — Raised when a new transformation is created
* TransformationUpdatedEvent — Raised when transformation metadata is updated
* TransformationStateChangedEvent — Raised when transformation lifecycle state transitions

## Specifications

* TransformationSpecification — Validates transformation structure and completeness

## Domain Services

* TransformationService — Domain operations for transformation management

## Invariants

* Business entities must remain consistent
* Relationships must be valid
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Created → Active → Updated → Inactive

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
