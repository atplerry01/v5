# Domain: Sourcing

## Classification

business-system

## Context

execution

## Status

S4

## Purpose

Defines the structure of business sourcing records — the origin and procurement references for business execution resources.

## Core Responsibilities

* Define the structural representation of sourcing records
* Track sourcing state and metadata
* Enforce sourcing invariants and validation rules
* Manage sourcing lifecycle through state transitions

## Aggregate(s)

* SourcingAggregate

  * Root aggregate representing a business sourcing instance and its lifecycle
  * State machine: Requested -> Sourced, Requested -> Failed
  * Failure transitions require a reason

## Entities

* None

## Value Objects

* SourcingId — Unique identifier for a sourcing instance
* SourcingRequestId — Reference to the originating sourcing request
* SourcingStatus — Enum representing lifecycle state (Requested, Sourced, Failed)

## Domain Events

* SourcingCreatedEvent — Raised when a new sourcing is created (enters Requested state)
* SourcingSourcedEvent — Raised when sourcing completes successfully (Requested -> Sourced)
* SourcingFailedEvent — Raised when sourcing fails (Requested -> Failed), captures reason

## Specifications

* CanSourceSpecification — Validates that sourcing can transition to Sourced (must be Requested)
* CanFailSpecification — Validates that sourcing can transition to Failed (must be Requested)
* IsSourcedSpecification — Checks whether sourcing has completed successfully

## Domain Services

* SourcingService — Domain operations for sourcing management

## Errors

* SourcingErrors — Static factory for domain exceptions
* SourcingDomainException — Domain-specific exception type

## Invariants

* SourcingId must not be empty
* SourcingRequestId must not be empty
* Status must be a defined enum value
* Only Requested state allows transitions to Sourced or Failed
* Failure reason must not be empty

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Requested -> Sourced (success path)
Requested -> Failed (failure path, requires reason)

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. This context defines business execution STATE, not runtime execution behaviour.
