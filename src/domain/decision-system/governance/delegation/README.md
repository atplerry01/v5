# Domain: Delegation

## Classification

decision-system

## Context

governance

## Purpose

Manages the delegation of decision-making authority between governance actors.

## Core Responsibilities

* Delegation creation and constraints
* Delegation chain tracking
* Delegation revocation

## Aggregate(s)

* DelegationAggregate
  * Enforces delegation lifecycle

## Entities

* None

## Value Objects

* DelegationId

## Domain Events

* DelegationCreatedEvent
* DelegationStateChangedEvent
* DelegationUpdatedEvent

## Specifications

* DelegationSpecification — delegation validation rules

## Domain Services

* DelegationService

## Invariants

* Delegations must specify scope limits
* Delegation chains must be traceable to source authority
* Delegations must have expiry or review dates
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

requested → granted → active → expired/revoked → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
