# Domain: Setup

## Classification

business-system

## Context

execution

## Status

S4 — Full state machine with validated value objects, domain events, specifications, and aggregate transitions.

## Purpose

Defines the structure of business setups — the initial configuration and preparation records for business execution.

## Core Responsibilities

* Define the structural representation of setup records
* Track setup state and metadata through a governed state machine
* Enforce setup invariants and validation rules
* Emit domain events on every state transition

## State Machine

```
Pending → Configured → Ready
```

| Transition   | Method        | Guard Specification        |
|-------------|---------------|----------------------------|
| Configure() | Configure()   | CanConfigureSpecification  |
| MarkReady()  | MarkReady()   | CanMarkReadySpecification  |

## Aggregate(s)

* SetupAggregate
  * Root aggregate representing a business setup instance and its lifecycle
  * Factory: `Create(SetupId, SetupTargetId)` — produces aggregate in Pending state
  * `Configure()` — transitions Pending to Configured
  * `MarkReady()` — transitions Configured to Ready

## Entities

* None

## Value Objects

* SetupId — Validated unique identifier for a setup instance (rejects Guid.Empty)
* SetupTargetId — Validated reference to the target of the setup (rejects Guid.Empty)
* SetupStatus — Enum: Pending, Configured, Ready

## Domain Events

* SetupCreatedEvent(SetupId, SetupTargetId) — Raised when a new setup is created
* SetupConfiguredEvent(SetupId) — Raised when setup is configured
* SetupReadyEvent(SetupId) — Raised when setup is marked ready

## Specifications

* CanConfigureSpecification — Satisfied when status is Pending
* CanMarkReadySpecification — Satisfied when status is Configured
* IsReadySpecification — Satisfied when status is Ready

## Error Catalog

* SetupErrors.MissingId — SetupId must not be empty
* SetupErrors.MissingTargetId — SetupTargetId must not be empty
* SetupErrors.AlreadyConfigured — Setup has already been configured
* SetupErrors.AlreadyReady — Setup is already marked as ready
* SetupErrors.InvalidStateTransition — Illegal transition from current status

## Domain Services

* SetupService — Reserved for domain operations for setup management

## Invariants

* SetupId must not be default/empty
* SetupTargetId must not be default/empty
* Status must be a defined SetupStatus enum value
* Cannot Configure unless status is Pending
* Cannot MarkReady unless status is Configured

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed. This context defines business execution STATE, not runtime execution behaviour.
