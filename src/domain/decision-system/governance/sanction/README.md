# Domain: Sanction

## Classification

decision-system

## Context

governance

## Purpose

Manages governance sanctions imposed for policy violations or non-compliance.

## Core Responsibilities

* Sanction determination and issuance
* Sanction enforcement tracking
* Sanction appeal and lift management

## Aggregate(s)

* SanctionAggregate
  * Enforces sanction lifecycle

## Entities

* None

## Value Objects

* SanctionId

## Domain Events

* SanctionCreatedEvent
* SanctionStateChangedEvent
* SanctionUpdatedEvent

## Specifications

* SanctionSpecification — sanction validation rules

## Domain Services

* SanctionService

## Invariants

* Sanctions must reference the violation
* Sanctions must have proportionate severity
* Sanctions must have defined duration or conditions for lifting
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

determined → issued → active → enforced → appealed/lifted → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
