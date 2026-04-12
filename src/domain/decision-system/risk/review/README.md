# Domain: Review

## Classification

decision-system

## Context

risk

## Purpose

Manages periodic risk review processes evaluating the risk landscape.

## Core Responsibilities

* Risk review scheduling and execution
* Risk posture evaluation
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

* Reviews must cover all active risks in scope
* Reviews must be completed within defined periods
* Review findings must generate actionable recommendations
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

scheduled → in-progress → findings-recorded → recommendations-issued → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
