# Domain: Grant

## Classification

trust-system

## Context

access

## Purpose

Represents the domain responsible for managing access grants — the explicit bestowment of access rights from one principal to another, or from a policy to a principal, within the trust boundary.

## Core Responsibilities

* Create and manage access grants with defined scope and duration
* Enforce grant validity rules and constraints
* Emit events when grants are created, modified, or revoked

## Aggregate(s)

* GrantAggregate
  * Enforces invariants around grant creation, scope, and lifecycle
  * Validates grant conditions before committing changes

## Entities

* None

## Value Objects

* GrantId — Strongly-typed identifier for a grant record

## Domain Events

* GrantCreatedEvent — Raised when a new grant is established
* GrantStateChangedEvent — Raised when grant state transitions
* GrantUpdatedEvent — Raised when an existing grant is modified

## Specifications

* GrantSpecification — Validates grant eligibility and scope criteria

## Domain Services

* GrantService — Coordinates grant creation and revocation logic

## Invariants

* A grant must reference a valid grantor, grantee, and permission scope
* Grants must not exceed the grantor's own permission boundaries
* Grant lifecycle transitions must pass pre-change validation

## Policy Dependencies

* Grant duration limits, delegation depth rules, scope restrictions (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Grantor and grantee resolution
* Permission — Defines the permissions being granted
* Authorization — Grants feed into authorization decisions
* Role — Role-scoped grants

## Lifecycle

Created → Active → Updated → Expired | Revoked. All transitions emit domain events and enforce invariants.

## Notes

Grants represent the positive conferral of access. They are distinct from permissions (which define what can be done) and authorizations (which evaluate whether access is allowed). Grant delegation depth and scope are WHYCEPOLICY controlled.
