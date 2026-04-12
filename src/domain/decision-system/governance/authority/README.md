# Domain: Authority

## Classification

decision-system

## Context

governance

## Purpose

Defines and manages decision-making authority within governance structures.

## Core Responsibilities

* Authority assignment and boundaries
* Delegation chain management
* Authority verification

## Aggregate(s)

* AuthorityAggregate
  * Enforces authority lifecycle

## Entities

* None

## Value Objects

* AuthorityId

## Domain Events

* AuthorityCreatedEvent
* AuthorityStateChangedEvent
* AuthorityUpdatedEvent

## Specifications

* AuthoritySpecification — authority validation rules

## Domain Services

* AuthorityService

## Invariants

* Authority must have defined scope
* Authority delegation must maintain chain of accountability
* Authority boundaries must not overlap
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

granted → active → delegated → revoked → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
