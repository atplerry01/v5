# Domain: GovernanceCycle

## Classification

decision-system

## Context

governance

## Purpose

Represents the recurring governance review and decision-making cycle.

## Core Responsibilities

* Cycle scheduling and initiation
* Cycle activity tracking
* Cycle completion and reporting

## Aggregate(s)

* GovernanceCycleAggregate
  * Enforces governance-cycle lifecycle

## Entities

* None

## Value Objects

* GovernanceCycleId

## Domain Events

* GovernanceCycleCreatedEvent
* GovernanceCycleStateChangedEvent
* GovernanceCycleUpdatedEvent

## Specifications

* GovernanceCycleSpecification — governance-cycle validation rules

## Domain Services

* GovernanceCycleService

## Invariants

* Cycles must have defined start and end dates
* All scheduled activities must be completed or deferred
* Cycle reports must be generated before closure
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

planned → initiated → in-progress → reporting → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
