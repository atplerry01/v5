# Domain: Profile

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing identity profiles — the descriptive attributes and metadata associated with an identity that are not security-critical but provide context for trust and access decisions.

## Core Responsibilities

* Manage profile attributes and metadata for identities
* Enforce profile completeness and validity rules
* Emit events when profile data is created or modified

## Aggregate(s)

* ProfileAggregate
  * Enforces invariants around profile creation and attribute validity
  * Validates profile data before committing changes

## Entities

* None

## Value Objects

* ProfileId — Strongly-typed identifier for a profile record

## Domain Events

* ProfileCreatedEvent — Raised when a new profile is established
* ProfileStateChangedEvent — Raised when profile state transitions
* ProfileUpdatedEvent — Raised when profile data is modified

## Specifications

* ProfileSpecification — Validates profile completeness and attribute criteria

## Domain Services

* ProfileService — Coordinates profile management logic

## Invariants

* A profile must be bound to exactly one identity
* Mandatory profile attributes must be populated before profile is considered complete
* Profile updates must not alter immutable attributes

## Policy Dependencies

* Required profile fields, attribute validation rules, profile completeness thresholds (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Profile owner
* Consent — Profile data usage governed by consent
* Verification — Profile attributes may require verification
* Governance — Profile data audit trail

## Lifecycle

Created → Incomplete → Complete → Updated → Archived. All transitions emit domain events and enforce invariants.

## Notes

Profiles hold descriptive, non-security-critical data. Security-sensitive identity material belongs in the credential or verification domains. Profile visibility and sharing rules are WHYCEPOLICY controlled.
