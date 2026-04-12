# Domain: Credential

## Classification

business-system

## Context

integration

## Boundary

This domain defines integration credential structure only and contains no encryption or authentication logic.

## Core Responsibilities

* Define the structural representation of integration credentials
* Track credential metadata and lifecycle state
* Emit domain events on credential state changes

## Aggregate(s)

* CredentialAggregate

  * Represents the root entity for an integration credential, encapsulating its identity, descriptor, and lifecycle state
  * Factory: `Register(id, descriptor)` — creates a new credential in `Registered` state
  * Transitions: `Activate()`, `Revoke()`

## Entities

* None

## Value Objects

* CredentialId — Validated Guid identifier for a credential instance (rejects empty)
* CredentialStatus — Enum: Registered, Active, Revoked
* CredentialDescriptor — Record struct with PartnerReference (Guid, non-empty) and CredentialType (string, non-empty)

## Domain Events

* CredentialRegisteredEvent — Raised when a new credential is registered
* CredentialActivatedEvent — Raised when a credential is activated
* CredentialRevokedEvent — Raised when a credential is revoked

## Specifications

* CanActivateSpecification — Satisfied when status is Registered
* CanRevokeSpecification — Satisfied when status is Active

## Domain Services

* CredentialService — Reserved (empty)

## Invariants

* CredentialId must not be empty
* CredentialDescriptor must be valid
* Status must be a defined enum value

## Lifecycle

TERMINAL: Registered -> Active -> Revoked

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Notes

Business-system defines structure only. No encryption, token generation, HTTP calls, or execution logic allowed.
