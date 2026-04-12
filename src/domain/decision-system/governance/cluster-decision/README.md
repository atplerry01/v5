# Domain: ClusterDecision

## Classification

decision-system

## Context

governance

## Purpose

Manages decisions that span multiple governance domains or require coordinated resolution.

## Core Responsibilities

* Cross-domain decision coordination
* Cluster consensus tracking
* Unified outcome determination

## Aggregate(s)

* ClusterDecisionAggregate
  * Enforces cluster-decision lifecycle

## Entities

* None

## Value Objects

* ClusterDecisionId

## Domain Events

* ClusterDecisionCreatedEvent
* ClusterDecisionStateChangedEvent
* ClusterDecisionUpdatedEvent

## Specifications

* ClusterDecisionSpecification — cluster-decision validation rules

## Domain Services

* ClusterDecisionService

## Invariants

* Cluster decisions must reference all participating domains
* Consensus rules must be defined before evaluation
* Partial consensus must not be treated as approval
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

initiated → domains-engaged → evaluating → consensus-reached/deadlocked → resolved

## Notes

Decision-system must remain stateless in execution but stateful in outcome tracking
