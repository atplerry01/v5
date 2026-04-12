# Domain: VarianceControl

## Classification

core-system

## Context

financialcontrol

## Purpose

Defines the foundational structure for variance detection and tolerance boundaries. Provides reusable variance primitives for detecting deviations from expected values.

## Core Responsibilities

* Define variance detection structure and tolerance boundary thresholds
* Track deviation state and tolerance enforcement
* Provide reusable variance primitives for cross-system deviation detection

## Aggregate(s)

* VarianceControlAggregate

  * Manages the lifecycle and state of a variance control instance

## Entities

* None

## Value Objects

* VarianceControlId — Unique identifier for a variance-control instance

## Domain Events

* VarianceControlCreatedEvent — Raised when a new variance-control is created
* VarianceControlUpdatedEvent — Raised when variance-control metadata is updated
* VarianceControlStateChangedEvent — Raised when variance-control lifecycle state transitions

## Specifications

* VarianceControlSpecification — Validates variance-control structure and completeness

## Domain Services

* VarianceControlService — Domain operations for variance-control management

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
