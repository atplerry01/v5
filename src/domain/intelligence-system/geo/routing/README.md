# Domain: Routing

## Classification

intelligence-system

## Context

geo

## Purpose

Defines the structure of geographic routing records — the path and route data between geographic points.

## Core Responsibilities

* Define routing structure and metadata
* Enforce routing invariants and validation rules
* Emit domain events on routing lifecycle transitions

## Aggregate(s)

* RoutingAggregate

  * Root aggregate representing a geographic routing record with its structure, metadata, and lifecycle state

## Entities

* None

## Value Objects

* RoutingId — Unique identifier for a routing instance

## Domain Events

* RoutingCreatedEvent — Raised when a new routing is created
* RoutingUpdatedEvent — Raised when routing metadata is updated
* RoutingStateChangedEvent — Raised when routing lifecycle state transitions

## Specifications

* RoutingSpecification — Validates routing structure and completeness

## Domain Services

* RoutingService — Domain operations for routing management

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
