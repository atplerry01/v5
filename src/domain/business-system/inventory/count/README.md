# Domain: Count

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of inventory counts — physical or cycle count records for verifying actual stock levels.

## Core Responsibilities

* Define the structural representation of inventory counts
* Maintain count identity and metadata integrity
* Emit domain events on count lifecycle transitions

## Aggregate(s)

* CountAggregate

  * Represents the root entity for an inventory count, encapsulating its identity, state, and lifecycle rules

## Entities

* None

## Value Objects

* CountId — Unique identifier for a count instance

## Domain Events

* CountCreatedEvent — Raised when a new count is created
* CountUpdatedEvent — Raised when count metadata is updated
* CountStateChangedEvent — Raised when count lifecycle state transitions

## Specifications

* CountSpecification — Validates count structure and completeness

## Domain Services

* CountService — Domain operations for count management

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
