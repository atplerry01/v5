# Domain: AuditLog

## Classification

decision-system

## Context

audit

## Purpose

Captures and maintains immutable audit log entries for decision traceability.

## Core Responsibilities

* Immutable log entry creation
* Chronological event recording
* Audit trail integrity maintenance

## Aggregate(s)

* AuditLogAggregate
  * Enforces audit-log lifecycle

## Entities

* None

## Value Objects

* AuditLogId

## Domain Events

* AuditLogCreatedEvent
* AuditLogStateChangedEvent
* AuditLogUpdatedEvent

## Specifications

* AuditLogSpecification — audit-log validation rules

## Domain Services

* AuditLogService

## Invariants

* Log entries must be immutable once created
* Logs must maintain chronological ordering
* Log entries must reference source decisions
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

created → recorded → sealed → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
