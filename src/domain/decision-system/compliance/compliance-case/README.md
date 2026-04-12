# Domain: ComplianceCase

## Classification

decision-system

## Context

compliance

## Purpose

Represents the lifecycle of a compliance investigation or review case.

## Core Responsibilities

* Compliance case initiation
* Investigation tracking
* Case resolution and outcome recording

## Aggregate(s)

* ComplianceCaseAggregate
  * Enforces compliance-case lifecycle

## Entities

* None

## Value Objects

* ComplianceCaseId

## Domain Events

* ComplianceCaseCreatedEvent
* ComplianceCaseStateChangedEvent
* ComplianceCaseUpdatedEvent

## Specifications

* ComplianceCaseSpecification — compliance-case validation rules

## Domain Services

* ComplianceCaseService

## Invariants

* Cases must reference applicable regulations
* Cases must track all compliance findings
* Case closure requires a formal determination
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

opened → investigated → findings-recorded → determined → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
