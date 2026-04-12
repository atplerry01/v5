# Domain: Quorum

## Classification

decision-system

## Context

governance

## Purpose

Manages quorum requirements and verification for governance decision-making bodies.

## Core Responsibilities

* Quorum threshold definition
* Quorum verification
* Quorum status tracking

## Aggregate(s)

* QuorumAggregate
  * Enforces quorum lifecycle

## Entities

* None

## Value Objects

* QuorumId

## Domain Events

* QuorumCreatedEvent
* QuorumStateChangedEvent
* QuorumUpdatedEvent

## Specifications

* QuorumSpecification — quorum validation rules

## Domain Services

* QuorumService

## Invariants

* Quorum thresholds must be defined before decisions
* Decisions without quorum are invalid
* Quorum changes must follow amendment procedures
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

defined → verified → met/not-met → recorded

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
