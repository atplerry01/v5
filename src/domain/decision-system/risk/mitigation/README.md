# Domain: Mitigation

## Classification

decision-system

## Context

risk

## Purpose

Tracks risk mitigation actions and their effectiveness in reducing identified risks.

## Core Responsibilities

* Mitigation plan creation
* Mitigation action tracking
* Mitigation effectiveness measurement

## Aggregate(s)

* MitigationAggregate
  * Enforces mitigation lifecycle

## Entities

* None

## Value Objects

* MitigationId

## Domain Events

* MitigationCreatedEvent
* MitigationStateChangedEvent
* MitigationUpdatedEvent

## Specifications

* MitigationSpecification — mitigation validation rules

## Domain Services

* MitigationService

## Invariants

* Mitigations must reference specific risks
* Mitigations must have defined completion criteria
* Mitigation effectiveness must be measured
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

planned → approved → in-progress → completed → effectiveness-measured → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
