# Domain: Secret

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration secrets — the references to sensitive credentials used in external system authentication.

## Boundary

This domain defines secret reference structure only and contains no encryption or vault access logic.

## Core Responsibilities

* Define the structural identity and metadata of integration secrets
* Track secret lifecycle states and transitions
* Maintain relationships between secrets and other integration entities

## Aggregate(s)

* Secret

  * Represents a reference to sensitive credentials used in external system authentication
  * Factory: `Store(SecretId, SecretDescriptor)`
  * Transitions: `Activate()`, `Rotate()`, `Retire()`

## Entities

* None

## Value Objects

* SecretId — Unique identifier for a secret instance (validated Guid, non-empty)
* SecretStatus — Lifecycle state enum: Stored, Active, Rotated, Retired
* SecretDescriptor — Record struct with OwnerReference (Guid, non-empty) and SecretType (string, non-empty)

## Domain Events

* SecretStoredEvent — Raised when a new secret reference is stored
* SecretActivatedEvent — Raised when a secret is activated
* SecretRotatedEvent — Raised when a secret is rotated
* SecretRetiredEvent — Raised when a secret is retired

## Specifications

* CanActivateSpecification — Status must be Stored
* CanRotateSpecification — Status must be Active
* CanRetireSpecification — Status must be Active or Rotated

## Domain Services

* SecretService — Reserved for domain operations (currently empty)

## Invariants

* SecretId must not be empty
* SecretDescriptor must not be default
* Status must be a defined enum value
* State transitions enforced via specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Stored -> Active -> Rotated -> Retired (Active can also go directly to Retired)

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
