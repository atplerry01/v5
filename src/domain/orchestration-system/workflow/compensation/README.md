# Domain: Compensation

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow compensation actions — the rollback or corrective actions defined for saga-style orchestration when steps fail.

## Core Responsibilities

* Define compensation action structure
* Track compensation targets and sequences
* Maintain compensation state within saga lifecycle

## Aggregate(s)

* CompensationAggregate

  * Represents workflow compensation container

## Entities

* None

## Value Objects

* CompensationId — Unique identifier for a compensation instance

## Domain Events

* CompensationCreatedEvent — Raised when a new compensation is created
* CompensationUpdatedEvent — Raised when compensation metadata is updated
* CompensationStateChangedEvent — Raised when compensation lifecycle state transitions

## Specifications

* CompensationSpecification — Validates compensation structure and completeness

## Domain Services

* CompensationService — Domain operations for compensation management

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
