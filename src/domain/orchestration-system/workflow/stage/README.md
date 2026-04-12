# Domain: Stage

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow stages — the high-level phases within a workflow that group related steps. Represents stage composition and ordering.

## Core Responsibilities

* Define stage structure and composition
* Track stage ordering within workflow
* Maintain stage lifecycle state

## Aggregate(s)

* StageAggregate

  * Represents workflow stage container

## Entities

* None

## Value Objects

* StageId — Unique identifier for a stage instance

## Domain Events

* StageCreatedEvent — Raised when a new stage is created
* StageUpdatedEvent — Raised when stage metadata is updated
* StageStateChangedEvent — Raised when stage lifecycle state transitions

## Specifications

* StageSpecification — Validates stage structure and completeness

## Domain Services

* StageService — Domain operations for stage management

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
