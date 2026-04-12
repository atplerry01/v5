# Domain: Registry

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing the identity registry — the canonical index of all registered identities within the trust system, providing lookup, uniqueness enforcement, and registration lifecycle management.

## Core Responsibilities

* Maintain the authoritative registry of all identities
* Enforce uniqueness and registration constraints
* Emit events when registry entries are added, modified, or removed

## Aggregate(s)

* RegistryAggregate
  * Enforces invariants around registration uniqueness and lifecycle
  * Validates registry operations before committing changes

## Entities

* None

## Value Objects

* RegistryId — Strongly-typed identifier for a registry entry

## Domain Events

* RegistryCreatedEvent — Raised when a new registry entry is added
* RegistryStateChangedEvent — Raised when registry entry state transitions
* RegistryUpdatedEvent — Raised when a registry entry is modified

## Specifications

* RegistrySpecification — Validates registration eligibility and uniqueness criteria

## Domain Services

* RegistryService — Coordinates registration and lookup logic

## Invariants

* Each identity must have at most one registry entry
* Registry entries must enforce global uniqueness of identity identifiers
* Deregistered entries must not be reused without explicit re-registration

## Policy Dependencies

* Registration approval rules, deregistration policies, identifier format requirements (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Registered identities
* Service Identity — Service registrations
* Verification — Registration may require verification
* Governance — Registration audit trail

## Lifecycle

Registered → Active → Updated → Deregistered. All transitions emit domain events and enforce invariants.

## Notes

The registry is the system of record for identity existence. It does not hold identity attributes (that is the profile domain) or credentials (that is the credential domain). It answers the question "does this identity exist?" Registration policies are WHYCEPOLICY controlled.
