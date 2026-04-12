# Domain: Import

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration imports — the inbound data ingestion records from external systems.

## Core Responsibilities

* Define the structural representation of integration imports
* Track import metadata and lifecycle state
* Emit domain events on import state changes

## Aggregate(s)

* ImportAggregate

  * Represents the root entity for an integration import, encapsulating its identity, metadata, and lifecycle state

## Entities

* None

## Value Objects

* ImportId — Unique identifier for an import instance

## Domain Events

* ImportCreatedEvent — Raised when a new import is created
* ImportUpdatedEvent — Raised when import metadata is updated
* ImportStateChangedEvent — Raised when import lifecycle state transitions

## Specifications

* ImportSpecification — Validates import structure and completeness

## Domain Services

* ImportService — Domain operations for import management

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
