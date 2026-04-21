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

## Value Objects

* ApprovalControlId — Unique identifier for an approval-control instance

## Domain Events

* ApprovalControlCreatedEvent — Raised when a new approval-control is created

## Specifications

* ApprovalControlSpecification — Validates approval-control structure and completeness

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
