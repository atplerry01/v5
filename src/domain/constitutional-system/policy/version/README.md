# Domain: Version

## Classification

constitutional-system

## Context

policy

## Purpose

Defines constitutional version control structure — the rules governing how policies are versioned over time. Versioning ensures that policy changes are traceable, auditable, and reversible while maintaining constitutional integrity.

## Core Responsibilities

* Define version structure and identity
* Define versioning rules and transitions
* Define governance authority references for version management

## Aggregate(s)

* VersionAggregate

  * Represents the policy version container

## Entities

* None

## Value Objects

* VersionId — Unique identifier for a version definition

## Domain Events

* VersionCreatedEvent — Raised when a new version is created
* VersionUpdatedEvent — Raised when version metadata is updated
* VersionStateChangedEvent — Raised when version lifecycle state transitions

## Specifications

* VersionSpecification — Validates version structure and completeness

## Domain Services

* VersionService — Domain operations for version management

## Invariants

* Versions must be immutable once activated (unless superseded by a new version)
* Versions must be traceable
* Versions must be policy-bound (WHYCEPOLICY)

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
