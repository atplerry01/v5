# Domain: Obligation

## Classification

decision-system

## Context

compliance

## Purpose

Tracks compliance obligations and their fulfillment status within decision processes.

## Core Responsibilities

* Obligation identification and assignment
* Fulfillment tracking
* Obligation breach detection

## Aggregate(s)

* ObligationAggregate
  * Enforces obligation lifecycle

## Entities

* None

## Value Objects

* ObligationId

## Domain Events

* ObligationCreatedEvent
* ObligationStateChangedEvent
* ObligationUpdatedEvent

## Specifications

* ObligationSpecification — obligation validation rules

## Domain Services

* ObligationService

## Invariants

* Obligations must reference source regulations
* Obligations must have defined fulfillment criteria
* Breach detection must trigger escalation
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

identified → assigned → tracked → fulfilled/breached → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
