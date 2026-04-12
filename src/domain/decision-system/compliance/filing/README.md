# Domain: Filing

## Classification

decision-system

## Context

compliance

## Purpose

Manages regulatory filing decisions and submission lifecycle.

## Core Responsibilities

* Filing preparation and validation
* Submission tracking
* Filing acceptance/rejection recording

## Aggregate(s)

* FilingAggregate
  * Enforces filing lifecycle

## Entities

* None

## Value Objects

* FilingId

## Domain Events

* FilingCreatedEvent
* FilingStateChangedEvent
* FilingUpdatedEvent

## Specifications

* FilingSpecification — filing validation rules

## Domain Services

* FilingService

## Invariants

* Filings must reference applicable jurisdiction
* Filings must meet submission deadlines
* Filing content must be validated before submission
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

drafted → validated → submitted → accepted/rejected → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
