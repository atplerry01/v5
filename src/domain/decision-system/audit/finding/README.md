# Domain: Finding

## Classification

decision-system

## Context

audit

## Purpose

Represents audit findings discovered during decision audit processes.

## Core Responsibilities

* Finding identification and classification
* Severity assessment
* Finding-to-case linkage

## Aggregate(s)

* FindingAggregate
  * Enforces finding lifecycle

## Entities

* None

## Value Objects

* FindingId

## Domain Events

* FindingCreatedEvent
* FindingStateChangedEvent
* FindingUpdatedEvent

## Specifications

* FindingSpecification — finding validation rules

## Domain Services

* FindingService

## Invariants

* Findings must be classified by severity
* Findings must reference the originating audit case
* Findings must have a determination status
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

identified → classified → assessed → determined → reported → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
