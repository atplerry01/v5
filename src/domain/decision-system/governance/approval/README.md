# Domain: Approval

## Classification

decision-system

## Context

governance

## Purpose

Represents approval decisions within governance workflows.

## Core Responsibilities

* Approval request processing
* Approval authority validation
* Approval outcome recording

## Aggregate(s)

* ApprovalAggregate
  * Enforces approval lifecycle

## Entities

* None

## Value Objects

* ApprovalId

## Domain Events

* ApprovalCreatedEvent
* ApprovalStateChangedEvent
* ApprovalUpdatedEvent

## Specifications

* ApprovalSpecification — approval validation rules

## Domain Services

* ApprovalService

## Invariants

* Approvals must verify authority level
* Approvals must record the approving party
* Conditional approvals must track conditions
* Decisions must be deterministic
* Decisions must be traceable
* Decisions must not bypass policy

## Policy Dependencies

* Decision rules governed by WHYCEPOLICY

## Integration Points

* constitutional-system (policy)
* trust-system (trust input)
* economic-system (impact)

## Lifecycle

requested → under-review → approved/rejected/conditional → finalised

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
