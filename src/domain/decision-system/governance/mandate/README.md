# Domain: Mandate

## Classification

decision-system

## Context

governance

## Purpose

Represents governance mandates that define required actions or decisions.

## Core Responsibilities

* Mandate issuance and scope definition
* Mandate compliance tracking
* Mandate enforcement recording

## Aggregate(s)

* MandateAggregate
  * Enforces mandate lifecycle

## Entities

* None

## Value Objects

* MandateId

## Domain Events

* MandateCreatedEvent
* MandateStateChangedEvent
* MandateUpdatedEvent

## Specifications

* MandateSpecification — mandate validation rules

## Domain Services

* MandateService

## Invariants

* Mandates must have clear directives
* Mandates must specify compliance criteria
* Mandate enforcement must be recorded
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

issued → active → compliance-tracked → fulfilled/enforced → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
