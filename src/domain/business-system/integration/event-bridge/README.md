# Domain: EventBridge

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines interaction contracts only and contains no transport or execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Disabled states.

## Purpose

Manages event mapping contracts for integration bridges. An event bridge defines how domain events are mapped across system boundaries, without containing any event publishing or transport logic.

## Core Responsibilities

* Define event bridge identity and mapping reference
* Track bridge lifecycle state (Defined → Active ↔ Disabled)
* Enforce mapping configuration before activation
* Ensure no transport or execution logic leaks into domain

## Aggregate(s)

* EventBridgeAggregate
  * Manages the lifecycle and integrity of an event bridge mapping

## Value Objects

* EventBridgeId — Unique identifier for an event bridge instance
* EventBridgeStatus — Enum for lifecycle state (Defined, Active, Disabled)
* EventMappingId — Reference to the event mapping configuration

## Domain Events

* EventBridgeCreatedEvent — Raised when a new event bridge is defined
* EventBridgeActivatedEvent — Raised when the bridge is activated
* EventBridgeDisabledEvent — Raised when the bridge is disabled

## Specifications

* CanActivateSpecification — Defined or Disabled bridges can be activated
* CanDisableSpecification — Only Active bridges can be disabled
* IsActiveSpecification — Checks if bridge is currently active

## Domain Services

* EventBridgeService — Domain operations for event bridge management

## Errors

* MissingId — EventBridgeId is required
* MissingMappingId — EventMappingId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Bridge already in Active state
* AlreadyDisabled — Bridge already in Disabled state

## Invariants

* EventBridgeId must not be null/default
* EventMappingId must not be null/default
* EventBridgeStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* adapter (bridges may use adapters for mapping translation)
* endpoint (bridges route mapped events to endpoints)

## Status

**S4 — Invariants + Specifications Complete**
