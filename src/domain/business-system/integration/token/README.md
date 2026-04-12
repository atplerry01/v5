# Domain: Token

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration tokens — the authentication token references used for external system access.

## Boundary

This domain defines token structure only and contains no token generation or cryptographic logic.

## Core Responsibilities

* Define the structural identity and metadata of integration tokens
* Track token lifecycle states and transitions via specifications

## Aggregate(s)

* Token

  * Represents an authentication token reference used for external system access
  * Factory: `Issue(id, descriptor)` — creates a new token in `Issued` status
  * Transitions: `Activate()`, `Expire()`, `Revoke()`

## Entities

* None

## Value Objects

* TokenId — Validated Guid identifier for a token instance
* TokenStatus — Enum: Issued, Active, Expired, Revoked
* TokenDescriptor — Record struct with PartnerReference (Guid, non-empty) and TokenType (string, non-empty)

## Domain Events

* TokenIssuedEvent — Raised when a new token is issued
* TokenActivatedEvent — Raised when a token is activated
* TokenExpiredEvent — Raised when a token expires
* TokenRevokedEvent — Raised when a token is revoked

## Specifications

* CanActivateSpecification — Status must be Issued
* CanExpireSpecification — Status must be Active
* CanRevokeSpecification — Status must be Active

## Domain Services

* TokenService — Reserved (empty)

## Invariants

* TokenId must not be empty
* TokenDescriptor must not be default
* State transitions are guarded by specifications
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Issued → Active → Expired (terminal) or Active → Revoked (terminal)

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
