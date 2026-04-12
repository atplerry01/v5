# Domain: Committee

## Classification

decision-system

## Context

governance

## Purpose

Represents governance committees that make collective decisions.

## Core Responsibilities

* Committee composition management
* Meeting and quorum tracking
* Committee decision recording

## Aggregate(s)

* CommitteeAggregate
  * Enforces committee lifecycle

## Entities

* None

## Value Objects

* CommitteeId

## Domain Events

* CommitteeCreatedEvent
* CommitteeStateChangedEvent
* CommitteeUpdatedEvent

## Specifications

* CommitteeSpecification — committee validation rules

## Domain Services

* CommitteeService

## Invariants

* Committees must have defined membership
* Decisions require quorum
* Committee actions must be recorded
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

established → active → convened → decision-recorded → adjourned

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
