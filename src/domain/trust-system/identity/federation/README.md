# Domain: Federation

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing identity federation — the linking of identities across trust boundaries, enabling cross-system identity resolution and trust propagation.

## Core Responsibilities

* Establish and manage federated identity links
* Enforce federation trust rules and scope boundaries
* Emit events when federation links are created, updated, or dissolved

## Aggregate(s)

* FederationAggregate
  * Enforces invariants around federation link creation and trust scope
  * Validates federation conditions before committing changes

## Entities

* None

## Value Objects

* FederationId — Strongly-typed identifier for a federation link

## Domain Events

* FederationCreatedEvent — Raised when a new federation link is established
* FederationStateChangedEvent — Raised when federation state transitions
* FederationUpdatedEvent — Raised when a federation link is modified

## Specifications

* FederationSpecification — Validates federation eligibility and trust scope criteria

## Domain Services

* FederationService — Coordinates federation link management logic

## Invariants

* A federation link must reference two valid identity endpoints
* Federation trust scope must not exceed the trust level of either party
* Dissolved federation links must cascade revocation to dependent grants

## Policy Dependencies

* Federation trust levels, allowed federation partners, scope restrictions (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Federated identity resolution
* Identity Graph — Federation links contribute to identity graph topology
* Trust — Federation trust levels feed into trust scoring
* Governance — Federation audit trail

## Lifecycle

Proposed → Established → Active → Updated → Dissolved. All transitions emit domain events and enforce invariants.

## Notes

Federation enables cross-boundary identity without identity duplication. It is a trust relationship, not an identity merge. Federation scope and partner policies are WHYCEPOLICY controlled.
