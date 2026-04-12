# Domain: Regulation

## Classification

decision-system

## Context

compliance

## Purpose

Represents regulatory rules and requirements that drive compliance decisions.

## Core Responsibilities

* Regulation capture and versioning
* Applicability determination
* Regulation-to-obligation mapping

## Aggregate(s)

* RegulationAggregate
  * Enforces regulation lifecycle

## Entities

* None

## Value Objects

* RegulationId

## Domain Events

* RegulationCreatedEvent
* RegulationStateChangedEvent
* RegulationUpdatedEvent

## Specifications

* RegulationSpecification — regulation validation rules

## Domain Services

* RegulationService

## Invariants

* Regulations must be versioned
* Active regulations must map to obligations
* Superseded regulations must not generate new obligations
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

enacted → active → amended → superseded → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
