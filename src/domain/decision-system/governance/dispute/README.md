# Domain: Dispute

## Classification

decision-system

## Context

governance

## Purpose

Manages formal disputes raised against governance decisions.

## Core Responsibilities

* Dispute filing and classification
* Dispute investigation coordination
* Dispute resolution recording

## Aggregate(s)

* DisputeAggregate
  * Enforces dispute lifecycle

## Entities

* None

## Value Objects

* DisputeId

## Domain Events

* DisputeCreatedEvent
* DisputeStateChangedEvent
* DisputeUpdatedEvent

## Specifications

* DisputeSpecification — dispute validation rules

## Domain Services

* DisputeService

## Invariants

* Disputes must reference the contested decision
* Disputes must be classified by type
* Resolution must include rationale
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

filed → accepted → investigating → mediated → resolved → closed

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
