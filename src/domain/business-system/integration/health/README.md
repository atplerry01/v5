# Domain: Health

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration health records — the status and availability tracking for external system connections.

## Core Responsibilities

* Define the structural representation of integration health records
* Track health metadata and lifecycle state
* Emit domain events on health state changes

## Aggregate(s)

* HealthAggregate

  * Represents the root entity for an integration health record, encapsulating its identity, metadata, and lifecycle state

## Entities

* None

## Value Objects

* HealthId — Unique identifier for a health instance

## Domain Events

* HealthCreatedEvent — Raised when a new health is created
* HealthUpdatedEvent — Raised when health metadata is updated
* HealthStateChangedEvent — Raised when health lifecycle state transitions

## Specifications

* HealthSpecification — Validates health structure and completeness

## Domain Services

* HealthService — Domain operations for health management

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
