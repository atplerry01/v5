# Domain: AuditCase

## Classification

decision-system

## Context

audit

## Purpose

Represents the lifecycle of an audit case from initiation through resolution.

## Core Responsibilities

* Audit case initiation and tracking
* Case assignment and ownership
* Case resolution and closure

## Aggregate(s)

* AuditCaseAggregate
  * Enforces audit-case lifecycle

## Entities

* None

## Value Objects

* AuditCaseId

## Domain Events

* AuditCaseCreatedEvent
* AuditCaseStateChangedEvent
* AuditCaseUpdatedEvent

## Specifications

* AuditCaseSpecification — audit-case validation rules

## Domain Services

* AuditCaseService

## Invariants

* Audit cases must have a defined scope
* Cases must track all findings
* Case closure requires resolution of all findings
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

initiated → assigned → in-progress → findings-recorded → resolved → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
