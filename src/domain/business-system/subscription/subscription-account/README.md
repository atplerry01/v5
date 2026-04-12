# Domain: SubscriptionAccount

## Classification

business-system

## Context

subscription

## Purpose

Defines the structure of subscription accounts — the root entity linking a subscriber to their active subscriptions.

## Boundary

This domain defines subscription account structure only and contains no billing or payment logic.

## Core Responsibilities

* Define and maintain subscription account structure
* Track the root entity linking subscribers to active subscriptions
* Enforce structural rules for subscription account definitions

## Aggregate(s)

* SubscriptionAccountAggregate

  * Root aggregate representing a structured subscription account and its lifecycle
  * Factory: `Open(id, holder)` — creates a new account in Created status
  * Transitions: `Activate()`, `Suspend()`, `Reactivate()`, `Close()`

## Entities

* None

## Value Objects

* SubscriptionAccountId — Unique identifier for a subscription-account instance (validated Guid, rejects empty)
* SubscriptionAccountStatus — Enum: Created, Active, Suspended, Closed
* AccountHolder — Record struct with HolderReference (Guid non-empty) and HolderName (string non-empty)

## Domain Events

* SubscriptionAccountOpenedEvent — Raised when a new subscription account is opened
* SubscriptionAccountActivatedEvent — Raised when a subscription account is activated (also raised on reactivation)
* SubscriptionAccountSuspendedEvent — Raised when a subscription account is suspended
* SubscriptionAccountClosedEvent — Raised when a subscription account is closed

## Specifications

* CanActivateSpecification — Status must be Created
* CanSuspendSpecification — Status must be Active
* CanReactivateSpecification — Status must be Suspended
* CanCloseSpecification — Status must be Active or Suspended

## Domain Services

* SubscriptionAccountService — Domain operations for subscription-account management

## Invariants

* Subscription account ID must not be empty
* Account holder must be present with valid reference and name
* Status must be a defined enum value
* No billing or payment logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

REVERSIBLE: Created -> Active <-> Suspended -> Closed

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
