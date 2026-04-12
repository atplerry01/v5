# Domain: Jurisdiction

## Classification

constitutional-system

## Context

policy

## Purpose

Defines constitutional jurisdiction boundaries — the scope and authority under which policies apply. Jurisdictions determine which rules govern which domains, contexts, or operational boundaries within the system.

## Core Responsibilities

* Define jurisdiction boundary structure and identity
* Define jurisdictional authority mappings
* Define governance authority references for jurisdictional scope

## Aggregate(s)

* JurisdictionAggregate

  * Represents the jurisdiction boundary container

## Entities

* None

## Value Objects

* JurisdictionId — Unique identifier for a jurisdiction definition

## Domain Events

* JurisdictionCreatedEvent — Raised when a new jurisdiction is defined
* JurisdictionUpdatedEvent — Raised when jurisdiction metadata is updated
* JurisdictionStateChangedEvent — Raised when jurisdiction lifecycle state transitions

## Specifications

* JurisdictionSpecification — Validates jurisdiction structure and completeness

## Domain Services

* JurisdictionService — Domain operations for jurisdiction management

## Invariants

* Jurisdictions must be immutable once activated (unless versioned)
* Jurisdictions must be traceable
* Jurisdictions must be policy-bound (WHYCEPOLICY)

## Policy Dependencies

* WHYCEPOLICY is the execution authority
* Domain only defines structure, not enforcement

## Integration Points

* decision-system
* trust-system
* economic-system
* WHYCEPOLICY engine (external)

## Lifecycle

Draft → Defined → Activated → Versioned → Deprecated

## Notes

This domain defines constitutional structure ONLY.
All enforcement is external via WHYCEPOLICY and runtime.
