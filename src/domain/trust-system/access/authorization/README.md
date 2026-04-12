# Domain: Authorization

## Classification

trust-system

## Context

access

## Boundary

This domain defines authorization structure only and contains no access control evaluation logic.

## Purpose

Represents the structural model of authorization decisions — whether access was granted, denied, or revoked for a given principal and resource. This domain does NOT perform permission checking or policy evaluation.

## Core Responsibilities

* Define the structural representation of authorization decisions
* Enforce lifecycle invariants on authorization state transitions
* Emit events when authorization state changes

## Aggregate(s)

* AuthorizationAggregate
  * Factory: Grant(id, scope) — creates a Granted authorization
  * Factory: Deny(id, scope) — creates a Denied authorization (terminal)
  * Transition: Revoke() — transitions Granted to Revoked (terminal)
  * Enforces invariants: valid id, valid scope, defined status

## Value Objects

* AuthorizationId — Strongly-typed validated Guid identifier
* AuthorizationStatus — Enum: Granted, Denied, Revoked
* AuthorizationScope — Record struct with PrincipalReference (Guid) and ResourceReference (string)

## Domain Events

* AuthorizationGrantedEvent(AuthorizationId, AuthorizationScope)
* AuthorizationDeniedEvent(AuthorizationId, AuthorizationScope)
* AuthorizationRevokedEvent(AuthorizationId)

## Specifications

* CanRevokeSpecification — Satisfied when status is Granted

## Domain Services

* AuthorizationService — Empty; no evaluation logic in this domain

## Errors

* MissingId — AuthorizationId is required
* MissingScope — AuthorizationScope is required
* InvalidStateTransition(status, action) — Invalid lifecycle transition

## Invariants

* AuthorizationId must not be empty
* AuthorizationScope must reference a valid principal and resource
* State transitions must respect lifecycle: Granted -> Revoked (terminal), Denied (terminal)

## Lifecycle

Granted -> Revoked (terminal). Denied is also terminal. All transitions emit domain events and enforce invariants.

## Policy Dependencies

* Access control policies (WHYCEPOLICY controlled at runtime)

## Notes

This domain does not perform authentication or authorization evaluation. It strictly defines the structure of authorization decisions. All policy enforcement is deferred to WHYCEPOLICY at runtime.
