# Domain: Subscription

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines coordination contracts only and contains no execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Deactivated states.

## Domain Responsibility

Models participation and interest declarations for integration. A subscription defines what data or events a participant is interested in, without executing any message dispatch or event propagation logic.

## Aggregate

* **SubscriptionAggregate** — Root aggregate managing subscription lifecycle.
  * Private constructor; created via `Create(SubscriptionId, SubscriptionTargetId)` factory method.
  * State transitions via `Activate()` and `Deactivate()` methods.
  * Event-sourced with optimistic concurrency via `Version`.

## State Model

```
Defined ──Activate()──> Active ──Deactivate()──> Deactivated ──Activate()──> Active
```

## Value Objects

* **SubscriptionId** — Deterministic identifier (validated non-empty Guid).
* **SubscriptionStatus** — Enum: `Defined`, `Active`, `Deactivated`.
* **SubscriptionTargetId** — Reference to the subscription interest target (validated non-empty Guid).

## Events

* **SubscriptionCreatedEvent** — Raised when subscription is defined (status: Defined).
* **SubscriptionActivatedEvent** — Raised when subscription is activated.
* **SubscriptionDeactivatedEvent** — Raised when subscription is deactivated.

## Invariants

* SubscriptionId must not be null/default.
* SubscriptionTargetId must not be null/default.
* SubscriptionStatus must be a defined enum value.

## Specifications

* **CanActivateSpecification** — Defined or Deactivated subscriptions can be activated (REVERSIBLE).
* **CanDeactivateSpecification** — Only Active subscriptions can be deactivated.
* **IsActiveSpecification** — Checks if subscription is currently active.

## Errors

* **MissingId** — SubscriptionId is required.
* **MissingTargetId** — SubscriptionTargetId is required.
* **AlreadyActive** — Subscription already active.
* **AlreadyDeactivated** — Subscription already deactivated.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Status

**S4 — Invariants + Specifications Complete**
