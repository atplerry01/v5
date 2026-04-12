# Domain: Assignment

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow task assignments — the association of work items to responsible actors or systems within an orchestration flow.

## Core Responsibilities

* Define assignment structure and identity
* Track assignment target references
* Maintain assignment state within workflow lifecycle

## Aggregate(s)

* AssignmentAggregate

  * Represents workflow assignment container

## Entities

* None

## Value Objects

* AssignmentId — Unique identifier for an assignment instance

## Domain Events

* AssignmentCreatedEvent — Raised when a new assignment is created
* AssignmentUpdatedEvent — Raised when assignment metadata is updated
* AssignmentStateChangedEvent — Raised when assignment lifecycle state transitions

## Specifications

* AssignmentSpecification — Validates assignment structure and completeness

## Domain Services

* AssignmentService — Domain operations for assignment management

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
