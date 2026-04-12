# Domain: AccessReview

## Classification

decision-system

## Context

governance

## Purpose

Manages periodic review of access decisions to ensure continued appropriateness.

## Core Responsibilities

* Access review scheduling
* Entitlement validation
* Review outcome recording

## Aggregate(s)

* AccessReviewAggregate
  * Enforces access-review lifecycle

## Entities

* None

## Value Objects

* AccessReviewId

## Domain Events

* AccessReviewCreatedEvent
* AccessReviewStateChangedEvent
* AccessReviewUpdatedEvent

## Specifications

* AccessReviewSpecification — access-review validation rules

## Domain Services

* AccessReviewService

## Invariants

* Reviews must cover all active access grants
* Reviews must be completed within defined periods
* Review outcomes must be actioned
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

scheduled → in-review → evaluated → certified/revoked → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
