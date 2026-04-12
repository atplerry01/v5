# Domain: Scope

## Classification

decision-system

## Context

governance

## Purpose

Defines and manages the scope boundaries for governance decisions and activities.

## Core Responsibilities

* Scope boundary definition
* Scope applicability determination
* Scope change management

## Aggregate(s)

* ScopeAggregate
  * Enforces scope lifecycle

## Entities

* None

## Value Objects

* ScopeId

## Domain Events

* ScopeCreatedEvent
* ScopeStateChangedEvent
* ScopeUpdatedEvent

## Specifications

* ScopeSpecification — scope validation rules

## Domain Services

* ScopeService

## Invariants

* Scope must have clear boundaries
* Scope changes must be versioned
* Out-of-scope decisions must be rejected
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

defined → active → amended → superseded → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
