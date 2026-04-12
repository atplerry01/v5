# Domain: Callback

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines interaction contracts only and contains no transport or execution logic.

## Lifecycle Pattern

**SEQUENTIAL** — Registered → Pending → Completed. No state reversal permitted.

## Purpose

Manages callback contracts for integration. A callback defines the contract structure for asynchronous response handling, tracked through registration, pending, and completion states.

## Core Responsibilities

* Define callback identity and contract definition
* Track callback lifecycle state (Registered → Pending → Completed)
* Enforce definition presence before state transitions
* Ensure sequential progression with no reversal

## Aggregate(s)

* CallbackAggregate
  * Manages the lifecycle and integrity of a callback contract

## Entities

* CallbackDefinition — Callback contract structure (DefinitionId, CallbackName, ContractDescription)

## Value Objects

* CallbackId — Unique identifier for a callback instance
* CallbackStatus — Enum for lifecycle state (Registered, Pending, Completed)
* CallbackDefinitionId — Reference to the callback definition

## Domain Events

* CallbackCreatedEvent — Raised when a new callback is registered
* CallbackMarkedPendingEvent — Raised when the callback is marked pending
* CallbackCompletedEvent — Raised when the callback is completed

## Specifications

* CanMarkPendingSpecification — Only Registered callbacks can be marked pending
* CanCompleteSpecification — Only Pending callbacks can be completed
* IsPendingSpecification — Checks if callback is currently pending

## Domain Services

* CallbackService — Domain operations for callback management

## Errors

* MissingId — CallbackId is required
* MissingDefinition — CallbackDefinition is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyPending — Callback already in Pending state
* AlreadyCompleted — Callback already completed

## Invariants

* CallbackId must not be null/default
* CallbackDefinition must not be null
* CallbackStatus must be a defined enum value
* Sequential progression enforced (no state reversal)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* endpoint (callbacks reference endpoints)
* contract (callbacks operate under integration contracts)

## Status

**S4 — Invariants + Specifications Complete**
