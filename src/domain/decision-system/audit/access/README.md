# Domain: Access

## Classification

decision-system

## Context

audit

## Purpose

Represents audit access control decisions and access grant/revoke lifecycle.

## Core Responsibilities

* Access decision evaluation
* Access grant/revoke tracking
* Access audit trail maintenance

## Aggregate(s)

* AccessAggregate
  * Enforces access lifecycle

## Entities

* None

## Value Objects

* AccessId

## Domain Events

* AccessCreatedEvent
* AccessStateChangedEvent
* AccessUpdatedEvent

## Specifications

* AccessSpecification — access validation rules

## Domain Services

* AccessService

## Invariants

* Access decisions must reference a valid audit scope
* Access grants must be time-bounded
* Revocations must be recorded with reason
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

requested → evaluated → granted/denied → revoked → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
