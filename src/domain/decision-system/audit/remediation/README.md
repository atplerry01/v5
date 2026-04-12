# Domain: Remediation

## Classification

decision-system

## Context

audit

## Purpose

Tracks remediation actions required to address audit findings.

## Core Responsibilities

* Remediation plan creation
* Action tracking and verification
* Remediation closure validation

## Aggregate(s)

* RemediationAggregate
  * Enforces remediation lifecycle

## Entities

* None

## Value Objects

* RemediationId

## Domain Events

* RemediationCreatedEvent
* RemediationStateChangedEvent
* RemediationUpdatedEvent

## Specifications

* RemediationSpecification — remediation validation rules

## Domain Services

* RemediationService

## Invariants

* Remediation must reference a specific finding
* Remediation actions must have defined completion criteria
* Closure requires verification of effectiveness
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

planned → assigned → in-progress → verified → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
