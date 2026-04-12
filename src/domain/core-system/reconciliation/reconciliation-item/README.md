# Domain: ReconciliationItem

## Classification

core-system

## Context

reconciliation

## Purpose

Defines the foundational structure for individual reconciliation items — the discrete elements being compared during a reconciliation process. Provides reusable item primitives for data matching.

## Core Responsibilities

* Represent discrete data elements subject to reconciliation comparison
* Provide structured matching primitives for cross-system data alignment
* Track item-level reconciliation state and comparison results

## Aggregate(s)

* ReconciliationItemAggregate

  * Manages the lifecycle and state of a reconciliation item instance

## Entities

* None

## Value Objects

* ReconciliationItemId — Unique identifier for a reconciliation item instance

## Domain Events

* ReconciliationItemCreatedEvent — Raised when a new reconciliation item is created
* ReconciliationItemUpdatedEvent — Raised when reconciliation item metadata is updated
* ReconciliationItemStateChangedEvent — Raised when reconciliation item lifecycle state transitions

## Specifications

* ReconciliationItemSpecification — Validates reconciliation item structure and completeness

## Domain Services

* ReconciliationItemService — Domain operations for reconciliation item management

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Created → Active → Updated → Deprecated

## Notes

Core-system must remain minimal, pure, and reusable.
