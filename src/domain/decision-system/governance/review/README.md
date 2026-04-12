# Domain: Review

## Classification

decision-system

## Context

governance

## Purpose

Manages governance review processes for decision evaluation and oversight.

## Core Responsibilities

* Review initiation and scoping
* Review assessment execution
* Review finding and recommendation recording

## Aggregate(s)

* ReviewAggregate
  * Enforces review lifecycle

## Entities

* None

## Value Objects

* ReviewId

## Domain Events

* ReviewCreatedEvent
* ReviewStateChangedEvent
* ReviewUpdatedEvent

## Specifications

* ReviewSpecification — review validation rules

## Domain Services

* ReviewService

## Invariants

* Reviews must have defined scope and criteria
* Reviews must produce documented findings
* Review recommendations must be tracked
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

initiated → scoped → assessing → findings-recorded → recommendations-issued → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
