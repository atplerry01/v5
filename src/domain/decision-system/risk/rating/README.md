# Domain: Rating

## Classification

decision-system

## Context

risk

## Purpose

Manages risk rating decisions that classify and score identified risks.

## Core Responsibilities

* Risk scoring and classification
* Rating methodology application
* Rating change tracking

## Aggregate(s)

* RatingAggregate
  * Enforces rating lifecycle

## Entities

* None

## Value Objects

* RatingId

## Domain Events

* RatingCreatedEvent
* RatingStateChangedEvent
* RatingUpdatedEvent

## Specifications

* RatingSpecification — rating validation rules

## Domain Services

* RatingService

## Invariants

* Ratings must use approved methodology
* Rating changes must be justified
* Ratings must be reviewed periodically
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

assessed → scored → classified → reviewed → updated/confirmed → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
