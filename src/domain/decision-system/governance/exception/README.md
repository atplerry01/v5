# Domain: Exception

## Classification

decision-system

## Context

governance

## Purpose

Manages exception requests to standard governance rules or policies.

## Core Responsibilities

* Exception request evaluation
* Risk-based exception approval
* Exception monitoring and expiry

## Aggregate(s)

* ExceptionAggregate
  * Enforces exception lifecycle

## Entities

* None

## Value Objects

* ExceptionId

## Domain Events

* ExceptionCreatedEvent
* ExceptionStateChangedEvent
* ExceptionUpdatedEvent

## Specifications

* ExceptionSpecification — exception validation rules

## Domain Services

* ExceptionService

## Invariants

* Exceptions must reference the rule being excepted
* Exceptions must have defined duration
* Exceptions must include compensating controls
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

requested → evaluated → approved/denied → active → expired → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
