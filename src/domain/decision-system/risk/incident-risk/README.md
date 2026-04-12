# Domain: IncidentRisk

## Classification

decision-system

## Context

risk

## Purpose

Manages risk decisions arising from security or operational incidents.

## Core Responsibilities

* Incident risk identification
* Impact assessment coordination
* Post-incident risk recalibration

## Aggregate(s)

* IncidentRiskAggregate
  * Enforces incident-risk lifecycle

## Entities

* None

## Value Objects

* IncidentRiskId

## Domain Events

* IncidentRiskCreatedEvent
* IncidentRiskStateChangedEvent
* IncidentRiskUpdatedEvent

## Specifications

* IncidentRiskSpecification — incident-risk validation rules

## Domain Services

* IncidentRiskService

## Invariants

* Incident risks must reference the source incident
* Impact assessments must be completed within defined timeframes
* Risk recalibration must update affected controls
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

identified → assessed → impact-determined → mitigations-applied → recalibrated → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
