# Domain: Credential

## Classification

trust-system

## Context

identity

## Boundary

This domain defines credential structure only and contains no cryptographic or authentication logic.

## Purpose

Represents the domain responsible for managing credential structure — the representation of proof materials bound to an identity. This domain does NOT perform encryption, hashing, or token generation.

## Core Responsibilities

* Define credential structure and identity binding
* Enforce credential lifecycle state transitions (Issued -> Active -> Revoked)
* Emit events when credentials are issued, activated, or revoked

## Aggregate(s)

* CredentialAggregate
  * Factory: Issue(id, descriptor) — creates a credential in Issued state
  * Transitions: Activate() and Revoke()
  * Enforces invariants and specification-gated transitions

## Entities

* None

## Value Objects

* CredentialId — Strongly-typed identifier (validated Guid, rejects Guid.Empty)
* CredentialStatus — Enum: Issued, Active, Revoked
* CredentialDescriptor — Record struct with IdentityReference (Guid, non-empty) and CredentialType (string, non-empty)

## Domain Events

* CredentialIssuedEvent — Raised when a new credential is issued
* CredentialActivatedEvent — Raised when a credential is activated
* CredentialRevokedEvent — Raised when a credential is revoked

## Specifications

* CanActivateSpecification — Satisfied when status is Issued
* CanRevokeSpecification — Satisfied when status is Active

## Domain Services

* CredentialService — Empty; reserved for future cross-aggregate coordination

## Errors

* MissingId — CredentialId is required and must not be empty
* MissingDescriptor — CredentialDescriptor is required and must not be default
* InvalidStateTransition(status, action) — InvalidOperationException for illegal lifecycle transitions

## Invariants

* A credential must have a non-default CredentialId
* A credential must have a non-default CredentialDescriptor
* CredentialStatus must be a defined enum value

## Lifecycle

TERMINAL: Issued -> Active -> Revoked. All transitions emit domain events and enforce invariants. No reverse transitions permitted.

## Policy Dependencies

* Credential lifecycle policies (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Owner of the credential via IdentityReference
* Verification — Credentials validated during verification flows

## Notes

This domain manages the domain model for credential structure, not cryptographic operations. Actual secret handling, hashing, and token generation are infrastructure concerns outside this boundary.
