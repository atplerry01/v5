# Domain: Version

## Maturity Level

S4 — Invariants + Specifications Complete

## Classification

business-system

## Context

document

## Purpose

Defines the structure of document versions — the revision history and version tracking for business documents. Manages version lifecycle through deterministic state transitions: Draft, Released, Superseded.

## Core Responsibilities

* Define version structure and metadata
* Track version identity and classification
* Maintain version lifecycle state (Draft → Released → Superseded)
* Enforce immutability on superseded versions
* Deterministic version numbering via VersionNumber

## Aggregate(s)

* VersionAggregate
  * Manages the lifecycle and integrity of a version entity
  * State model: Draft → Released → Superseded
  * Emits domain events on state transitions
  * Enforces invariants: valid VersionId, defined VersionStatus

## Entities

* VersionMetadata — Holds version number, lineage identifier, and optional parent version reference

## Value Objects

* VersionId — Unique identifier for a version instance (must not be empty)
* VersionNumber — Deterministic major.minor version numbering with increment operations
* VersionStatus — Enum representing lifecycle state: Draft, Released, Superseded

## Domain Events

* VersionCreatedEvent(VersionId) — Raised when a new version is created
* VersionReleasedEvent(VersionId) — Raised when a draft version is released
* VersionSupersededEvent(VersionId) — Raised when a released version is superseded

## Specifications

* CanReleaseSpecification — Validates that a version is in Draft status before release
* CanSupersedeSpecification — Validates that a version is in Released status before superseding
* IsImmutableSpecification — Validates that a superseded version cannot be modified

## Domain Errors

* VersionErrors — Static factory for domain-specific exceptions
* VersionDomainException — Sealed exception type for version domain violations

## Domain Services

* VersionService — Domain operations for version management (placeholder)

## Invariants

* VersionId must not be empty (default)
* VersionStatus must be a defined enum value
* Metadata must be assigned before release
* Superseded versions are immutable — no modifications allowed
* Version number increments deterministically
* Cannot release from any state other than Draft
* Cannot supersede from any state other than Released

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY
* ValidateBeforeChange provides policy hook for runtime enforcement

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Draft → Released → Superseded

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. Zero external dependencies in the domain layer.
