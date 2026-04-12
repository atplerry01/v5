# Domain: Subscription

## Classification

business-system

## Context

notification

## Domain Responsibility

Defines opt-in/opt-out relationships — links subscribers to notification channels or topics. This domain defines notification structure only and contains no execution logic.

## Aggregate

* **SubscriptionAggregate** — Root aggregate representing a subscription relationship.
  * Private constructor; created via `Create(SubscriptionId, SubscriptionTarget)` factory method.
  * State transitions via `OptOut()` and `Resubscribe()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entities

* None

## State Model

```
OptedIn ──OptOut()──> OptedOut ──Resubscribe()──> OptedIn
```

## Value Objects

* **SubscriptionId** — Deterministic identifier (validated non-empty Guid).
* **SubscriptionStatus** — Enum: `OptedIn`, `OptedOut`.
* **SubscriptionTarget** — Target reference and type (channel or topic).

## Events

* **SubscriptionOptedInEvent** — Raised when a subscription is created (status: OptedIn).
* **SubscriptionOptedOutEvent** — Raised when subscriber opts out.
* **SubscriptionResubscribedEvent** — Raised when subscriber resubscribes.

## Invariants

* SubscriptionId must not be null/default.
* SubscriptionTarget must define a valid reference and type.
* SubscriptionStatus must be a defined enum value.
* Must not execute notification routing — relationship definition only.
* State transitions enforced by specifications.

## Specifications

* **CanOptOutSpecification** — Only OptedIn subscriptions can opt out.
* **CanResubscribeSpecification** — Only OptedOut subscriptions can resubscribe.

## Errors

* **MissingId** — SubscriptionId is required.
* **InvalidTarget** — Subscription must define a valid target.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services

* **SubscriptionService** — Reserved for cross-aggregate coordination within subscription context.

## Lifecycle Pattern

REVERSIBLE — Subscriptions can be opted out and resubscribed.

## Boundary Statement

This domain defines notification structure only and contains no execution logic.

## Status

**S4 — Invariants + Specifications Complete**
