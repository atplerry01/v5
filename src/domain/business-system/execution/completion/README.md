# Domain: Completion

## Classification

business-system

## Context

execution

## Purpose

Manages the outcome tracking for business execution phases. Records whether an execution target completed successfully or failed, with mandatory failure reasoning. Requires a valid target reference for every completion record.

## Core Responsibilities

* Define completion identity and target reference
* Track completion lifecycle state (Pending → Completed or Failed)
* Enforce that completion and failure are mutually exclusive terminal states
* Capture failure reason when execution fails

## Aggregate(s)

* CompletionAggregate
  * Manages the lifecycle and outcome of a business execution completion

## Value Objects

* CompletionId — Unique identifier for a completion instance
* CompletionStatus — Enum for lifecycle state (Pending, Completed, Failed)
* CompletionTargetId — Reference to the execution target being tracked

## Domain Events

* CompletionCreatedEvent — Raised when a new completion is created
* CompletionCompletedEvent — Raised when execution completes successfully
* CompletionFailedEvent — Raised when execution fails (includes reason)

## Specifications

* CanCompleteSpecification — Only Pending completions can be completed
* CanFailSpecification — Only Pending completions can fail
* IsCompletedSpecification — Checks if completion was successful

## Domain Services

* CompletionService — Domain operations for completion management

## Errors

* MissingId — CompletionId is required
* MissingTargetId — CompletionTargetId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyCompleted — Completion already in Completed state
* AlreadyFailed — Completion already in Failed state

## Invariants

* CompletionId must not be null/default
* CompletionTargetId must not be null/default
* CompletionStatus must be a defined enum value
* Cannot complete twice (enforced by CanCompleteSpecification)
* Fail only from Pending (enforced by CanFailSpecification)
* Failure reason must not be empty when failing
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* activation (completion follows activation)
* lifecycle (completion marks lifecycle phase end)

## Status

**S4 — Invariants + Specifications Complete**
