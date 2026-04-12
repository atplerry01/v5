# Domain: Transition

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow transitions — the valid state changes between steps or stages within a workflow. Represents transition rules and guards, not transition execution.

## Core Responsibilities

* Define transition structure and rules
* Track transition source and target references
* Maintain transition guard conditions

## Aggregate(s)

* TransitionAggregate

  * Represents workflow transition container

## Entities

* None

## Value Objects

* TransitionId — Unique identifier for a transition instance

## Domain Events

* TransitionCreatedEvent — Raised when a new transition is created
* TransitionUpdatedEvent — Raised when transition metadata is updated
* TransitionStateChangedEvent — Raised when transition lifecycle state transitions

## Specifications

* TransitionSpecification — Validates transition structure and completeness

## Domain Services

* TransitionService — Domain operations for transition management

## Invariants

* Orchestration must be deterministic
* State transitions must be valid
* No execution logic inside domain

## Policy Dependencies

* Workflow constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system
* operational-system
* runtime (external execution only)

## Lifecycle

Defined → Started → In-Progress → Completed → Failed → Terminated

## Notes

This domain defines orchestration structure ONLY. Execution is handled externally by engines/runtime.
