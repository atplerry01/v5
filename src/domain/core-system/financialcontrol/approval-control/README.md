# Domain: ApprovalControl

## Classification

core-system

## Context

financialcontrol

## Purpose

Defines the foundational structure for approval workflows and control gates. Provides reusable approval primitives for cross-system authorization flows.

## Core Responsibilities

* Define approval workflow structure and gate sequencing
* Track approval state transitions and authorization outcomes
* Provide reusable approval primitives for cross-system authorization flows

## Aggregate(s)

* ApprovalControlAggregate

  * Manages the lifecycle and state of an approval control instance

## Entities

* None

## Value Objects

* ApprovalControlId — Unique identifier for an approval-control instance

## Domain Events

* ApprovalControlCreatedEvent — Raised when a new approval-control is created
* ApprovalControlUpdatedEvent — Raised when approval-control metadata is updated
* ApprovalControlStateChangedEvent — Raised when approval-control lifecycle state transitions

## Specifications

* ApprovalControlSpecification — Validates approval-control structure and completeness

## Domain Services

* ApprovalControlService — Domain operations for approval-control management

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
