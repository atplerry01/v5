# Domain: Allocation

## Classification

business-system

## Context

execution

## Purpose

Manages assignment of execution resources or capacity to business execution activities. Distinct from entitlement/allocation — this represents runtime execution resource allocation, not rights allocation. Tracks capacity requests and enforces resource reference integrity.

## Core Responsibilities

* Define allocation identity and execution resource reference
* Track allocation lifecycle state (Pending → Allocated → Released)
* Enforce capacity constraints on allocations
* Ensure release only occurs after allocation

## Aggregate(s)

* AllocationAggregate
  * Manages the lifecycle and integrity of an execution resource allocation

## Value Objects

* AllocationId — Unique identifier for an allocation instance
* AllocationStatus — Enum for lifecycle state (Pending, Allocated, Released)
* ExecutionResourceId — Reference to the execution resource being allocated

## Domain Events

* AllocationCreatedEvent — Raised when a new allocation is created
* AllocationAllocatedEvent — Raised when allocation is confirmed
* AllocationReleasedEvent — Raised when allocation is released

## Specifications

* CanAllocateSpecification — Only Pending allocations can be allocated
* CanReleaseSpecification — Only Allocated allocations can be released
* IsAllocatedSpecification — Checks if allocation is currently active

## Domain Services

* AllocationService — Domain operations for allocation management

## Errors

* MissingId — AllocationId is required
* MissingResourceId — ExecutionResourceId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* CapacityExceeded — Requested capacity exceeds available
* AlreadyAllocated — Allocation already in Allocated state
* AlreadyReleased — Allocation already in Released state

## Invariants

* AllocationId must not be null/default
* ExecutionResourceId must not be null/default
* AllocationStatus must be a defined enum value
* Requested capacity must be greater than zero
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* activation (allocation supports active execution)
* charge (allocation may incur charges)

## Status

**S4 — Invariants + Specifications Complete**
