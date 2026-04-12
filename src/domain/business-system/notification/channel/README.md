# Domain: Channel

## Classification

business-system

## Context

notification

## Domain Responsibility

Defines the structure of notification channels — the delivery mediums through which notifications are sent (email, SMS, push, webhook). This domain defines notification structure only and contains no execution logic.

## Aggregate

* **ChannelAggregate** — Root aggregate representing a notification channel definition.
  * Private constructor; created via `Create(ChannelId, ChannelType)` factory method.
  * State transitions via `Activate()` and `Deactivate()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## Entities

* None

## State Model

```
Draft ──Activate()──> Active ──Deactivate()──> Deactivated (terminal)
```

## Value Objects

* **ChannelId** — Deterministic identifier (validated non-empty Guid).
* **ChannelStatus** — Enum: `Draft`, `Active`, `Deactivated`.
* **ChannelType** — Communication medium type (e.g., email, sms, push, webhook).

## Events

* **ChannelRegisteredEvent** — Raised when a new channel is registered (status: Draft).
* **ChannelActivatedEvent** — Raised when channel is activated.
* **ChannelDeactivatedEvent** — Raised when channel is deactivated (terminal).

## Invariants

* ChannelId must not be null/default.
* ChannelType must not be empty.
* ChannelStatus must be a defined enum value.
* Must not perform message sending — structure definition only.
* State transitions enforced by specifications.

## Specifications

* **CanActivateSpecification** — Only Draft channels can be activated.
* **CanDeactivateSpecification** — Only Active channels can be deactivated.

## Errors

* **MissingId** — ChannelId is required.
* **InvalidChannelType** — Channel must define a valid type.
* **InvalidStateTransition** — Guard for illegal status transitions.

## Domain Services

* **ChannelService** — Reserved for cross-aggregate coordination within channel context.

## Lifecycle Pattern

TERMINAL — Once deactivated, a channel cannot be reactivated.

## Boundary Statement

This domain defines notification structure only and contains no execution logic.

## Status

**S4 — Invariants + Specifications Complete**
