# Domain: Vote

## Classification

decision-system

## Context

governance

## Purpose

Manages voting decisions within governance processes.

## Core Responsibilities

* Vote casting and validation
* Vote counting and tallying
* Vote outcome determination

## Aggregate(s)

* VoteAggregate
  * Enforces vote lifecycle

## Entities

* None

## Value Objects

* VoteId

## Domain Events

* VoteCreatedEvent
* VoteStateChangedEvent
* VoteUpdatedEvent

## Specifications

* VoteSpecification — vote validation rules

## Domain Services

* VoteService

## Invariants

* Votes must verify voter eligibility
* Each voter may cast only one vote per decision
* Vote tallies must be verifiable
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

opened → ballots-cast → tallied → outcome-determined → certified → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
