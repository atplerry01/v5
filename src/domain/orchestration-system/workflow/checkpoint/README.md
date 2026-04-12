# Domain: Checkpoint

## Classification

orchestration-system

## Context

workflow

## Purpose

Defines the structure of workflow checkpoints — the saved progress markers within an orchestration flow that enable resumability and auditability.

## Core Responsibilities

* Define checkpoint structure and identity
* Track workflow progress markers
* Maintain checkpoint state for resumability

## Aggregate(s)

* CheckpointAggregate

  * Represents workflow checkpoint container

## Entities

* None

## Value Objects

* CheckpointId — Unique identifier for a checkpoint instance

## Domain Events

* CheckpointCreatedEvent — Raised when a new checkpoint is created
* CheckpointUpdatedEvent — Raised when checkpoint metadata is updated
* CheckpointStateChangedEvent — Raised when checkpoint lifecycle state transitions

## Specifications

* CheckpointSpecification — Validates checkpoint structure and completeness

## Domain Services

* CheckpointService — Domain operations for checkpoint management

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
