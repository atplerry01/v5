# Domain: Job

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of async integration jobs — the identity, descriptor, and lifecycle states of job units.

## Boundary

This domain defines async job structure only and contains no job execution or background processing logic.

## Core Responsibilities

* Define the structural identity and metadata of integration jobs
* Track job lifecycle states and transitions (Created, Running, Completed, Failed)
* Enforce valid state transitions via specifications

## Aggregate(s)

* Job

  * Represents an async processing unit for integration work
  * Factory: Create(id, descriptor)
  * Transitions: Start(), Complete(), Fail()

## Entities

* None

## Value Objects

* JobId — Validated unique identifier for a job instance
* JobStatus — Enum: Created, Running, Completed, Failed
* JobDescriptor — Record struct with JobName and JobType

## Domain Events

* JobCreatedEvent — Raised when a new job is created
* JobStartedEvent — Raised when a job transitions to running
* JobCompletedEvent — Raised when a job completes successfully
* JobFailedEvent — Raised when a job fails

## Specifications

* CanStartSpecification — Status must be Created
* CanCompleteSpecification — Status must be Running
* CanFailSpecification — Status must be Running

## Domain Services

* JobService — Empty; reserved for future domain operations

## Invariants

* JobId must not be empty
* JobDescriptor must not be default
* Status must be a defined enum value
* State transitions must follow lifecycle rules

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Lifecycle

Created → Running → Completed | Failed

## Notes

Business-system defines structure only. No execution, scheduling, or background processing logic allowed.
