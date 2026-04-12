# Domain: Resolution

## Classification

decision-system

## Context

governance

## Purpose

Represents formal resolutions adopted by governance bodies.

## Core Responsibilities

* Resolution drafting and adoption
* Resolution implementation tracking
* Resolution supersession management

## Aggregate(s)

* ResolutionAggregate
  * Enforces resolution lifecycle

## Entities

* None

## Value Objects

* ResolutionId

## Domain Events

* ResolutionCreatedEvent
* ResolutionStateChangedEvent
* ResolutionUpdatedEvent

## Specifications

* ResolutionSpecification — resolution validation rules

## Domain Services

* ResolutionService

## Invariants

* Resolutions must be formally adopted
* Resolutions must have defined effective dates
* Superseded resolutions must reference successors
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

drafted → proposed → adopted → active → superseded/rescinded → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
