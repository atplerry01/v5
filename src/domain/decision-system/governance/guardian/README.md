# Domain: Guardian

## Classification

decision-system

## Context

governance

## Purpose

Represents governance guardians responsible for oversight and protection of decision integrity.

## Core Responsibilities

* Guardian assignment and scope
* Oversight action tracking
* Guardian accountability recording

## Aggregate(s)

* GuardianAggregate
  * Enforces guardian lifecycle

## Entities

* None

## Value Objects

* GuardianId

## Domain Events

* GuardianCreatedEvent
* GuardianStateChangedEvent
* GuardianUpdatedEvent

## Specifications

* GuardianSpecification — guardian validation rules

## Domain Services

* GuardianService

## Invariants

* Guardians must have defined oversight scope
* Guardian actions must be recorded
* Guardian assignments must not create conflicts of interest
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

appointed → active → oversight-recorded → reassigned/retired → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
