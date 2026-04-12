# Domain: Appeal

## Classification

decision-system

## Context

governance

## Purpose

Manages the appeal process for contested governance decisions.

## Core Responsibilities

* Appeal submission and validation
* Appeal review coordination
* Appeal outcome determination

## Aggregate(s)

* AppealAggregate
  * Enforces appeal lifecycle

## Entities

* None

## Value Objects

* AppealId

## Domain Events

* AppealCreatedEvent
* AppealStateChangedEvent
* AppealUpdatedEvent

## Specifications

* AppealSpecification — appeal validation rules

## Domain Services

* AppealService

## Invariants

* Appeals must reference an original decision
* Appeals must be filed within allowed timeframes
* Appeal outcomes must be recorded with rationale
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

submitted → accepted → under-review → determined → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
