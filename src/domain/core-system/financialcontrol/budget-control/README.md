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

## Entities

* None

## Value Objects

* BudgetControlId — Unique identifier for a budget-control instance

## Domain Events

* BudgetControlCreatedEvent — Raised when a new budget-control is created
* BudgetControlUpdatedEvent — Raised when budget-control metadata is updated
* BudgetControlStateChangedEvent — Raised when budget-control lifecycle state transitions

## Specifications

* BudgetControlSpecification — Validates budget-control structure and completeness

## Domain Services

* BudgetControlService — Domain operations for budget-control management

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
