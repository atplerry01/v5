# Domain: Instance

## Classification

orchestration-system

## Context

workflow

## Purpose

This domain defines workflow instance structure only and contains no workflow execution or scheduling logic.

## Core Responsibilities

* Define workflow instance structure
* Track instance-specific context and parameters
* Maintain instance lifecycle state

## Aggregate(s)

* InstanceAggregate

  * Factory: Create(id, context)
  * Transitions: Start(), Complete(), Fail(), Terminate()

## Entities

* None

## Value Objects

* InstanceId — Validated Guid identifier (rejects Guid.Empty)
* InstanceStatus — Enum: Created, Running, Completed, Failed, Terminated
* InstanceContext — Record struct with DefinitionReference (Guid) and InstanceName (string)

## Domain Events

* InstanceCreatedEvent(InstanceId, InstanceContext) — Raised when a new instance is created
* InstanceStartedEvent(InstanceId) — Raised when instance transitions to Running
* InstanceCompletedEvent(InstanceId) — Raised when instance completes successfully
* InstanceFailedEvent(InstanceId) — Raised when instance fails
* InstanceTerminatedEvent(InstanceId) — Raised when instance is terminated

## Specifications

* CanStartSpecification — Status must be Created
* CanCompleteSpecification — Status must be Running
* CanFailSpecification — Status must be Running
* CanTerminateSpecification — Status must not be Completed or Terminated

## Domain Services

* InstanceService — Empty; no domain service operations required

## Errors

* MissingId — InstanceId cannot be empty
* MissingContext — InstanceContext is invalid
* InvalidStateTransition(status, action) — InvalidOperationException for disallowed transitions

## Invariants

* InstanceId must be non-empty Guid
* InstanceContext requires non-empty DefinitionReference and InstanceName
* State transitions must follow the lifecycle graph
* No execution logic inside domain

## Lifecycle

SEQUENTIAL with branches: Created -> Running -> Completed | Failed | Terminated

## Policy Dependencies

* Workflow constraints may be governed by WHYCEPOLICY

## Integration Points

* decision-system
* operational-system
* runtime (external execution only)

## Notes

This domain defines orchestration structure ONLY. Execution is handled externally by engines/runtime.
