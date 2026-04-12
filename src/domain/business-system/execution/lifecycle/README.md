# Domain: Lifecycle

## Classification

business-system

## Context

execution

## Purpose

Manages the state machine governing how business entities progress through execution stages. Tracks deterministic lifecycle progression and enforces that states cannot be skipped. Requires a valid subject reference for every lifecycle instance.

## Core Responsibilities

* Define lifecycle identity and subject reference
* Track lifecycle progression (Initialized → Running → Completed or Terminated)
* Enforce sequential state progression (no skipping states)
* Ensure completion and termination are mutually exclusive terminal outcomes

## Aggregate(s)

* LifecycleAggregate
  * Manages the lifecycle progression and integrity of an execution lifecycle

## Value Objects

* LifecycleId — Unique identifier for a lifecycle instance
* LifecycleStatus — Enum for lifecycle state (Initialized, Running, Completed, Terminated)
* LifecycleSubjectId — Reference to the entity whose lifecycle is being tracked

## Domain Events

* LifecycleCreatedEvent — Raised when a new lifecycle is created
* LifecycleStartedEvent — Raised when lifecycle execution begins
* LifecycleCompletedEvent — Raised when lifecycle completes successfully
* LifecycleTerminatedEvent — Raised when lifecycle is terminated

## Specifications

* CanStartSpecification — Only Initialized lifecycles can be started
* CanCompleteSpecification — Only Running lifecycles can be completed
* CanTerminateSpecification — Only Running lifecycles can be terminated
* IsRunningSpecification — Checks if lifecycle is currently running

## Domain Services

* LifecycleService — Domain operations for lifecycle management

## Errors

* MissingId — LifecycleId is required
* MissingSubjectId — LifecycleSubjectId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyRunning — Lifecycle already in Running state
* AlreadyCompleted — Lifecycle already in Completed state
* AlreadyTerminated — Lifecycle already in Terminated state

## Invariants

* LifecycleId must not be null/default
* LifecycleSubjectId must not be null/default
* LifecycleStatus must be a defined enum value
* Cannot skip states: Initialized→Completed forbidden (must go through Running)
* Complete and Terminate only from Running (enforced by specifications)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* activation (lifecycle may trigger activations)
* completion (lifecycle completion feeds completion records)
* cost (lifecycle stages may incur costs)

## Status

**S4 — Invariants + Specifications Complete**
