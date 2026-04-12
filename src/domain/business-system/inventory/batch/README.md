# Domain: Batch

## Classification

business-system

## Context

inventory

## Purpose

Defines the structure of inventory batches — groups of inventory units received or produced together. A batch groups valid items and cannot be modified after closure.

## Core Responsibilities

* Define the structural representation of inventory batches
* Enforce batch grouping and closure immutability
* Emit domain events on batch lifecycle transitions

## Aggregate(s)

* BatchAggregate
  * Represents the root entity for an inventory batch, encapsulating its identity, grouping, and lifecycle rules

## Entities

* None

## Value Objects

* BatchId — Unique identifier for a batch instance
* BatchStatus — Lifecycle state (Open, Closed)

## Domain Events

* BatchCreatedEvent — Raised when a new batch is created
* BatchClosedEvent — Raised when a batch is closed
* BatchUpdatedEvent — Raised when batch metadata is updated
* BatchStateChangedEvent — Raised when batch lifecycle state transitions

## Specifications

* CanCloseBatchSpecification — Guards transition from Open to Closed
* BatchSpecification — Validates batch structure and completeness

## Domain Services

* BatchService — Coordination placeholder for batch domain operations

## Invariants

* Batch must have a valid identity (non-empty BatchId)
* Must group valid items
* Cannot be modified after closure (TERMINAL)
* Status must be a defined enum value

## Boundary Statement

This domain owns batch grouping definition only. Batch defines which items belong together but does not own item logic. No implicit stock changes.

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* item (batch references items)
* structural-system

## Lifecycle

Open → Closed

**Pattern: TERMINAL** — Once closed, batch cannot be reopened or modified.

## Status

**S4 — Invariants + Specifications Complete**
