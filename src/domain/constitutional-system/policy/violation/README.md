# Domain: Violation

## Classification

constitutional-system

## Context

policy

## Purpose

Defines the constitutional violation structure — the shape of policy violation records within the system. Violations represent detected breaches of constitutional rules, captured as structured records for audit and traceability purposes.

## Core Responsibilities

* Define violation record structure and identity
* Define violation classification rules
* Define governance authority references for violation tracking

## Aggregate(s)

* ViolationAggregate

  * Represents the violation record container

## Entities

* None

## Value Objects

* ViolationId — Unique identifier for a violation record

## Domain Events

* ViolationCreatedEvent — Raised when a new violation is recorded
* ViolationUpdatedEvent — Raised when violation metadata is updated
* ViolationStateChangedEvent — Raised when violation lifecycle state transitions

## Specifications

* ViolationSpecification — Validates violation record structure and completeness

## Domain Services

* ViolationService — Domain operations for violation record management

## Invariants

* Violation records must be immutable once created
* Violation records must be traceable to the source rule and decision
* Violation records must be policy-bound (WHYCEPOLICY)

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines structure, not enforcement

## Integration Points

* decision-system
* trust-system
* economic-system
* WHYCEPOLICY engine (external)

## Lifecycle

Draft → Defined → Activated → Versioned → Deprecated

## Notes

This domain defines constitutional structure ONLY.
All enforcement is external via WHYCEPOLICY and runtime.
