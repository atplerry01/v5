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

## Value Objects

* SpendControlId — Unique identifier for a spend-control instance

## Domain Events

* SpendControlCreatedEvent — Raised when a new spend-control is created

## Specifications

* SpendControlSpecification — Validates spend-control structure and completeness

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

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Notes

Core-system must remain minimal, pure, and reusable.
