# Domain: Consent

## Classification

trust-system

## Context

identity

## Purpose

Represents the domain responsible for managing consent records — explicit agreements by an identity to allow specific uses of their data, capabilities, or trust relationships within the system.

## Core Responsibilities

* Capture and manage consent grants from identities
* Track consent scope, duration, and revocation
* Emit events when consent is given, modified, or withdrawn

## Aggregate(s)

* ConsentAggregate
  * Enforces invariants around consent creation and validity
  * Validates consent conditions before committing changes

## Entities

* None

## Value Objects

* ConsentId — Strongly-typed identifier for a consent record

## Domain Events

* ConsentCreatedEvent — Raised when consent is granted
* ConsentStateChangedEvent — Raised when consent state transitions
* ConsentUpdatedEvent — Raised when consent scope is modified

## Specifications

* ConsentSpecification — Validates consent eligibility and scope criteria

## Domain Services

* ConsentService — Coordinates consent capture and evaluation logic

## Invariants

* Consent must reference a valid identity and a defined purpose
* Consent cannot be granted on behalf of another identity without delegation authority
* Withdrawn consent must be honoured immediately across all dependent systems

## Policy Dependencies

* Consent expiry durations, mandatory consent requirements, delegation rules (WHYCEPOLICY controlled)

## Integration Points

* Identity (WhyceID) — Identity granting consent
* Governance — Compliance audit trail for consent decisions
* Profile — Consent may govern profile data usage

## Lifecycle

Granted → Active → Updated → Withdrawn | Expired. All transitions emit domain events and enforce invariants.

## Notes

Consent is a compliance-critical domain. It records the affirmative agreement of an identity to specific data or capability usage. Consent withdrawal triggers cascading policy enforcement. All consent rules are WHYCEPOLICY controlled.
