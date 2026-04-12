# Domain: CommandBridge

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines interaction contracts only and contains no transport or execution logic.

## Lifecycle Pattern

**REVERSIBLE** — Can toggle between Active and Disabled states.

## Purpose

Manages command mapping contracts for integration bridges. A command bridge defines how commands are mapped across system boundaries, without containing any command execution or transport logic.

## Core Responsibilities

* Define command bridge identity and mapping reference
* Track bridge lifecycle state (Defined → Active ↔ Disabled)
* Enforce mapping configuration before activation
* Ensure no transport or execution logic leaks into domain

## Aggregate(s)

* CommandBridgeAggregate
  * Manages the lifecycle and integrity of a command bridge mapping

## Value Objects

* CommandBridgeId — Unique identifier for a command bridge instance
* CommandBridgeStatus — Enum for lifecycle state (Defined, Active, Disabled)
* CommandMappingId — Reference to the command mapping configuration

## Domain Events

* CommandBridgeCreatedEvent — Raised when a new command bridge is defined
* CommandBridgeActivatedEvent — Raised when the bridge is activated
* CommandBridgeDisabledEvent — Raised when the bridge is disabled

## Specifications

* CanActivateSpecification — Defined or Disabled bridges can be activated
* CanDisableSpecification — Only Active bridges can be disabled
* IsActiveSpecification — Checks if bridge is currently active

## Domain Services

* CommandBridgeService — Domain operations for command bridge management

## Errors

* MissingId — CommandBridgeId is required
* MissingMappingId — CommandMappingId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Bridge already in Active state
* AlreadyDisabled — Bridge already in Disabled state

## Invariants

* CommandBridgeId must not be null/default
* CommandMappingId must not be null/default
* CommandBridgeStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* adapter (bridges may use adapters for mapping translation)
* endpoint (bridges route mapped commands to endpoints)

## Status

**S4 — Invariants + Specifications Complete**
