# Domain: Step

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow steps — the individual units of work within a workflow stage. Represents step definition and state, not step execution behaviour.

## Core Responsibilities

* Define step structure and identity
* Track step definition and parameters
* Maintain step lifecycle state

## Aggregate(s)

* StepAggregate

  * Represents workflow step container

## Entities

* None

## Value Objects

* StepId — Unique identifier for a step instance

## Domain Events

* StepCreatedEvent — Raised when a new step is created
* StepUpdatedEvent — Raised when step metadata is updated
* StepStateChangedEvent — Raised when step lifecycle state transitions

## Specifications

* StepSpecification — Validates step structure and completeness

## Domain Services

* StepService — Domain operations for step management

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
