# Domain: BudgetControl

## Classification

core-system

## Context

financialcontrol

## Purpose

Defines the foundational structure for budget boundaries and control limits. Provides reusable budget primitives for tracking allocation and consumption.

## Core Responsibilities

* Define budget boundary structure and control limit thresholds
* Track budget allocation and consumption state
* Provide reusable budget primitives for cross-system boundary enforcement

## Aggregate(s)

* BudgetControlAggregate

  * Manages the lifecycle and state of a budget control instance

## Value Objects

* BudgetControlId — Unique identifier for a budget-control instance

## Domain Events

* BudgetControlCreatedEvent — Raised when a new budget-control is created

## Specifications

* BudgetControlSpecification — Validates budget-control structure and completeness

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
