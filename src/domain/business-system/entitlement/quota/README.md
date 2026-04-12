# Domain: Quota

## Classification

business-system

## Context

entitlement

## Purpose

Manages usage caps and consumption tracking for entitlements. A quota defines a total capacity and tracks consumption against it, transitioning through availability, consumption, and exhaustion states.

## Core Responsibilities

* Define quota identity, subject, and total capacity
* Track consumption against capacity via QuotaUsage entities
* Enforce that consumption cannot exceed capacity
* Determine exhaustion deterministically

## Aggregate(s)

* QuotaAggregate
  * Manages the lifecycle and integrity of a usage quota

## Entities

* QuotaUsage — Tracks individual consumption units against the quota

## Value Objects

* QuotaId — Unique identifier for a quota instance
* QuotaStatus — Enum for lifecycle state (Available, Consumed, Exhausted)
* QuotaSubjectId — Reference to the subject the quota applies to

## Domain Events

* QuotaCreatedEvent — Raised when a new quota is created
* QuotaConsumedEvent — Raised when units are consumed against the quota
* QuotaExhaustedEvent — Raised when the quota is fully exhausted

## Specifications

* CanConsumeSpecification — Only Available quotas can be consumed
* CanExhaustSpecification — Only Consumed quotas can be exhausted
* IsAvailableSpecification — Checks if quota has remaining capacity

## Domain Services

* QuotaService — Domain operations for quota management

## Errors

* MissingId — QuotaId is required
* MissingSubjectId — QuotaSubjectId is required
* InvalidCapacity — TotalCapacity must be greater than zero
* InvalidStateTransition — Attempted transition not allowed from current status
* CapacityExceeded — Requested consumption exceeds remaining capacity
* AlreadyExhausted — Quota already in Exhausted state

## Invariants

* QuotaId must not be null/default
* QuotaSubjectId must not be null/default
* TotalCapacity must be greater than zero
* TotalConsumed cannot exceed TotalCapacity
* QuotaStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* limit (quotas may be constrained by limits)
* allocation (quota consumption may trigger allocation)

## Status

**S4 — Invariants + Specifications Complete**
