# Domain: Assessment

## Classification

decision-system

## Context

risk

## Purpose

Represents risk assessment decisions evaluating potential threats and their impact.

## Core Responsibilities

* Risk identification and evaluation
* Impact and likelihood assessment
* Assessment outcome determination

## Aggregate(s)

* AssessmentAggregate
  * Enforces assessment lifecycle

## Entities

* None

## Value Objects

* AssessmentId

## Domain Events

* AssessmentCreatedEvent
* AssessmentStateChangedEvent
* AssessmentUpdatedEvent

## Specifications

* AssessmentSpecification — assessment validation rules

## Domain Services

* AssessmentService

## Invariants

* Assessments must evaluate both impact and likelihood
* Assessments must reference applicable risk criteria
* Assessment methodology must be consistent
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

initiated → data-gathered → evaluated → scored → determined → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
