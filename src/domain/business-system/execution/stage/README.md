# Domain: Stage

## Classification

business-system

## Context

execution

## Status

S4

## Purpose

Defines the structure of business stages — the distinct phases within a business execution lifecycle.

## Core Responsibilities

* Define the structural representation of stage records
* Track stage state and metadata
* Enforce stage invariants and validation rules
* Enforce strict state progression (Initialized -> InProgress -> Completed)

## Aggregate(s)

* StageAggregate

  * Root aggregate representing a business stage instance and its lifecycle
  * State machine: Initialized -> InProgress -> Completed
  * Strict progression enforced — cannot skip to Completed without starting

## Entities

* None

## Value Objects

* StageId — Unique identifier for a stage instance
* StageContextId — Reference to the context a stage belongs to
* StageStatus — Enum representing stage lifecycle state (Initialized, InProgress, Completed)

## Domain Events

* StageCreatedEvent — Raised when a new stage is created
* StageStartedEvent — Raised when a stage transitions to InProgress
* StageCompletedEvent — Raised when a stage transitions to Completed

## Specifications

* CanStartSpecification — Validates that a stage can transition to InProgress (must be Initialized)
* CanCompleteSpecification — Validates that a stage can transition to Completed (must be InProgress)
* IsCompletedSpecification — Checks whether a stage has reached Completed status

## Domain Services

* StageService — Domain operations for stage management

## Errors

* StageErrors — Static factory for domain-specific error conditions
* StageDomainException — Typed exception for stage domain violations

## Invariants

* StageId must not be empty
* StageContextId must not be empty
* Status must be a defined StageStatus value
* State transitions must follow strict progression: Initialized -> InProgress -> Completed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Initialized -> InProgress -> Completed

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. This context defines business execution STATE, not runtime execution behaviour.
