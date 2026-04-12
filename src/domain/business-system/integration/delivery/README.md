# Domain: Delivery

## Classification

business-system

## Context

integration

## Boundary

This domain defines delivery contract structure only and contains no message delivery execution logic.

## Core Responsibilities

* Define the structural representation of message delivery contracts
* Track delivery lifecycle state (Scheduled, Dispatched, Confirmed, Failed)
* Emit domain events on delivery state transitions

## Aggregate(s)

* DeliveryAggregate

  * Factory: `Schedule(DeliveryId, DeliveryDescriptor)` — creates a new delivery in Scheduled state
  * Transitions: `Dispatch()`, `Confirm()`, `Fail()`
  * Lifecycle: Scheduled -> Dispatched -> Confirmed | Failed

## Entities

* None

## Value Objects

* DeliveryId — Validated Guid identifier for a delivery instance (non-empty)
* DeliveryStatus — Enum: Scheduled, Dispatched, Confirmed, Failed
* DeliveryDescriptor — Record struct with TargetReference (Guid, non-empty) and PayloadType (string, non-empty)

## Domain Events

* DeliveryScheduledEvent(DeliveryId, DeliveryDescriptor) — Raised when a delivery is scheduled
* DeliveryDispatchedEvent(DeliveryId) — Raised when a delivery is dispatched
* DeliveryConfirmedEvent(DeliveryId) — Raised when a delivery is confirmed
* DeliveryFailedEvent(DeliveryId) — Raised when a delivery fails

## Errors

* MissingId — DeliveryId is required and must not be empty
* MissingDescriptor — DeliveryDescriptor is required
* InvalidStateTransition(status, action) — InvalidOperationException when a transition violates lifecycle rules

## Specifications

* CanDispatchSpecification — Satisfied when status is Scheduled
* CanConfirmSpecification — Satisfied when status is Dispatched
* CanFailSpecification — Satisfied when status is Dispatched

## Domain Services

* DeliveryService — Empty; delivery contract structure is fully expressed through the aggregate

## Invariants

* DeliveryId must be non-empty at all times
* TargetReference must be non-empty
* PayloadType must be non-empty
* State transitions must follow the lifecycle: Scheduled -> Dispatched -> Confirmed | Failed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. No HTTP calls, no message delivery execution.
