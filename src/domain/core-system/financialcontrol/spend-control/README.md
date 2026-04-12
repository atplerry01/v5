# Domain: SpendControl

## Classification

core-system

## Context

financialcontrol

## Purpose

Defines the foundational structure for spend tracking and control limits. Provides reusable spend primitives for expenditure boundary management.

## Core Responsibilities

* Define spend tracking structure and control limit thresholds
* Track expenditure state and boundary enforcement
* Provide reusable spend primitives for cross-system expenditure management

## Aggregate(s)

* SpendControlAggregate

  * Manages the lifecycle and state of a spend control instance

## Entities

* None

## Value Objects

* SpendControlId — Unique identifier for a spend-control instance

## Domain Events

* SpendControlCreatedEvent — Raised when a new spend-control is created
* SpendControlUpdatedEvent — Raised when spend-control metadata is updated
* SpendControlStateChangedEvent — Raised when spend-control lifecycle state transitions

## Specifications

* SpendControlSpecification — Validates spend-control structure and completeness

## Domain Services

* SpendControlService — Domain operations for spend-control management

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
