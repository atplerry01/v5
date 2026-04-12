# Domain: Adapter

## Classification

business-system

## Context

integration

## Purpose

Manages integration adapter definitions — the translation components that map between external and internal data formats. Tracks adapter lifecycle from defined through active to disabled. No infrastructure or network logic.

## Core Responsibilities

* Define adapter identity and type reference
* Track adapter lifecycle state (Defined → Active → Disabled)
* Enforce that activation requires a valid type definition
* Disabled adapters cannot process interactions

## Aggregate(s)

* AdapterAggregate
  * Manages the lifecycle and configuration of an integration adapter

## Value Objects

* AdapterId — Unique identifier for an adapter instance
* AdapterStatus — Enum for lifecycle state (Defined, Active, Disabled)
* AdapterTypeId — Reference to the adapter type definition

## Domain Events

* AdapterCreatedEvent — Raised when a new adapter is created
* AdapterActivatedEvent — Raised when adapter is activated
* AdapterDisabledEvent — Raised when adapter is disabled

## Specifications

* CanActivateSpecification — Only Defined adapters can be activated
* CanDisableSpecification — Only Active adapters can be disabled
* IsActiveSpecification — Checks if adapter is currently active

## Domain Services

* AdapterService — Reserved for cross-aggregate coordination

## Errors

* MissingId — AdapterId is required
* MissingTypeId — AdapterTypeId is required
* InvalidStateTransition — Invalid state transition attempted
* AlreadyActive — Adapter already active
* AlreadyDisabled — Adapter already disabled

## Invariants

* AdapterId must not be null/default
* AdapterTypeId must not be null/default
* AdapterStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* client (adapters may serve client connections)
* endpoint (adapters translate for endpoints)

## Status

**S4 — Invariants + Specifications Complete**
