# Domain: TimeWindow

## Classification

core-system

## Context

temporal

## Purpose

Defines the foundational structure for time windows — bounded temporal ranges used for windowed operations, aggregations, and validity periods. Provides reusable time window primitives.

## Core Responsibilities

* Define bounded temporal ranges for windowed operations and aggregations
* Provide reusable primitives for validity period calculations and overlap detection
* Support consistent time window arithmetic across all system components

## Aggregate(s)

* TimeWindowAggregate

  * Represents a time window instance and its bounded temporal range

## Entities

* None

## Value Objects

* TimeWindowId — Unique identifier for a time-window instance

## Domain Events

* TimeWindowCreatedEvent — Raised when a new time-window is created
* TimeWindowUpdatedEvent — Raised when time-window metadata is updated
* TimeWindowStateChangedEvent — Raised when time-window lifecycle state transitions

## Specifications

* TimeWindowSpecification — Validates time-window structure and completeness

## Domain Services

* TimeWindowService — Domain operations for time-window management

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
