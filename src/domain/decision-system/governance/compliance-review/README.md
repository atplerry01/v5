# Domain: ComplianceReview

## Classification

decision-system

## Context

governance

## Purpose

Manages governance-level compliance review decisions.

## Core Responsibilities

* Compliance review initiation
* Review assessment coordination
* Compliance determination recording

## Aggregate(s)

* ComplianceReviewAggregate
  * Enforces compliance-review lifecycle

## Entities

* None

## Value Objects

* ComplianceReviewId

## Domain Events

* ComplianceReviewCreatedEvent
* ComplianceReviewStateChangedEvent
* ComplianceReviewUpdatedEvent

## Specifications

* ComplianceReviewSpecification — compliance-review validation rules

## Domain Services

* ComplianceReviewService

## Invariants

* Reviews must reference applicable governance rules
* Reviews must assess against defined criteria
* Non-compliance must trigger remediation
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

initiated → assessing → findings-recorded → determined → remediation-tracked → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
