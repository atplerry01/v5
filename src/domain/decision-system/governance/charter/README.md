# Domain: Charter

## Classification

decision-system

## Context

governance

## Purpose

Represents governance charters that define the operating rules for decision bodies.

## Core Responsibilities

* Charter creation and ratification
* Operating rule definition
* Charter amendment tracking

## Aggregate(s)

* CharterAggregate
  * Enforces charter lifecycle

## Entities

* None

## Value Objects

* CharterId

## Domain Events

* CharterCreatedEvent
* CharterStateChangedEvent
* CharterUpdatedEvent

## Specifications

* CharterSpecification — charter validation rules

## Domain Services

* CharterService

## Invariants

* Charters must define decision-making procedures
* Charter amendments must follow ratification process
* Only one active charter version per body
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

drafted → ratified → active → amended → superseded → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
