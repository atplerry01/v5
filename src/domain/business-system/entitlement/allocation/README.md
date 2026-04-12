# Domain: Allocation

## Classification

business-system

## Context

entitlement

## Purpose

Manages assignment of resources or rights to parties. Tracks the lifecycle of an allocation from creation through active allocation to eventual release.

## Core Responsibilities

* Define allocation identity and resource reference
* Track allocation lifecycle state (Pending -> Allocated -> Released)
* Enforce capacity constraints on allocations
* Ensure release only occurs after allocation

## Aggregate(s)

* AllocationAggregate
  * Manages the lifecycle and integrity of a resource allocation

## Value Objects

* AllocationId -- Unique identifier for an allocation instance
* AllocationStatus -- Enum for lifecycle state (Pending, Allocated, Released)
* ResourceId -- Reference to the allocated resource or right

## Domain Events

* AllocationCreatedEvent -- Raised when a new allocation is created
* AllocationAllocatedEvent -- Raised when the allocation is confirmed
* AllocationReleasedEvent -- Raised when the allocation is released

## Specifications

* CanAllocateSpecification -- Only Pending allocations can be allocated
* CanReleaseSpecification -- Only Allocated allocations can be released
* IsAllocatedSpecification -- Checks if allocation is currently active

## Domain Services

* AllocationService -- Domain operations for allocation management

## Errors

* MissingId -- AllocationId is required
* MissingResourceId -- ResourceId is required
* InvalidStateTransition -- Attempted transition not allowed from current status
* CapacityExceeded -- Requested capacity exceeds available
* AlreadyAllocated -- Allocation already in Allocated state
* AlreadyReleased -- Allocation already in Released state

## Invariants

* AllocationId must not be null/default
* ResourceId must not be null/default
* AllocationStatus must be a defined enum value
* Requested capacity must be greater than zero
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* eligibility (determines if allocation is permitted)
* entitlement-grant (allocation may fulfill a grant)

## Status

**S4 -- Invariants + Specifications Complete**
