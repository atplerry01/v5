# Domain: Jurisdiction

## Classification

decision-system

## Context

compliance

## Purpose

Represents jurisdictional boundaries that govern compliance decisions.

## Core Responsibilities

* Jurisdiction definition and scope
* Regulatory applicability determination
* Cross-jurisdiction conflict resolution

## Aggregate(s)

* JurisdictionAggregate
  * Enforces jurisdiction lifecycle

## Entities

* None

## Value Objects

* JurisdictionId

## Domain Events

* JurisdictionCreatedEvent
* JurisdictionStateChangedEvent
* JurisdictionUpdatedEvent

## Specifications

* JurisdictionSpecification — jurisdiction validation rules

## Domain Services

* JurisdictionService

## Invariants

* Jurisdictions must have defined regulatory scope
* Jurisdiction boundaries must not overlap ambiguously
* Regulatory changes must be versioned
* Decisions must be deterministic
* Decisions must be traceable
* Decisions must not bypass policy

## Policy Dependencies

* Decision rules governed by WHYCEPOLICY

## Integration Points

* constitutional-system (policy)
* trust-system (trust input)
* economic-system (impact)

## Lifecycle

defined → active → amended → superseded → archived

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
