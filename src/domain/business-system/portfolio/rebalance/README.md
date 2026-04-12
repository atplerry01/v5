# Domain: Rebalance

## Classification

business-system

## Context

portfolio

## Purpose

Defines the structure of portfolio rebalancing intent — the target state adjustments required to realign portfolio composition with desired allocations.

## Boundary Statement

This domain defines portfolio behavior contracts only and contains no execution logic.

## Core Responsibilities

* Define and maintain rebalance intent structure and metadata
* Track rebalance lifecycle state transitions (submission, approval, rejection)
* Enforce structural invariants for rebalance consistency
* Must not execute rebalancing or perform optimization

## Aggregate(s)

* RebalanceAggregate
  * Represents the root entity for a portfolio rebalance record, encapsulating its structure and lifecycle

## Value Objects

* RebalanceId — Unique identifier for a rebalance instance
* RebalanceName — Descriptive name for the rebalancing intent
* RebalanceStatus — Lifecycle state (Draft, Pending, Approved, Rejected, Cancelled)

## Domain Events

* RebalanceCreatedEvent — Raised when a new rebalance intent is created
* RebalanceSubmittedEvent — Raised when a rebalance is submitted for approval
* RebalanceApprovedEvent — Raised when a rebalance is approved
* RebalanceRejectedEvent — Raised when a rebalance is rejected
* RebalanceCancelledEvent — Raised when a rebalance is cancelled
* RebalanceUpdatedEvent — Raised when rebalance metadata is updated
* RebalanceStateChangedEvent — Raised when rebalance lifecycle state transitions

## Specifications

* RebalanceSpecification — Validates rebalance structure and completeness
* CanSubmitSpecification — Draft/Rejected → Pending transition guard (reversible from rejection)
* CanApproveSpecification — Pending → Approved transition guard
* CanRejectSpecification — Pending → Rejected transition guard
* CanCancelSpecification — Draft/Pending → Cancelled transition guard

## Domain Services

* RebalanceService — Reserved for cross-aggregate coordination within rebalance context

## Invariants

* RebalanceId must not be empty
* RebalanceName must not be empty
* Status must be a defined enum value
* Must define target state, must not execute rebalancing
* No financial or execution logic allowed

## Lifecycle Pattern

REVERSIBLE: Draft → Pending → Approved/Rejected, Rejected → Pending (resubmit), Draft/Pending → Cancelled

* Draft: Initial state upon creation
* Pending: Submitted for review/approval
* Approved: Rebalance intent accepted (terminal)
* Rejected: Rebalance intent declined (can resubmit to Pending)
* Cancelled: Abandoned before approval (terminal)

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
