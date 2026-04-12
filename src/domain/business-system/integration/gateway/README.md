# Domain: Gateway

## Classification

business-system

## Context

integration

## Purpose

Manages integration entry points and routing capabilities. A gateway defines how external traffic is routed into the system, tracked through definition, activation, and disablement.

## Core Responsibilities

* Define gateway identity and routing reference
* Track gateway lifecycle state (Defined → Active → Disabled)
* Enforce route configuration before activation
* Ensure disabled gateways cannot route traffic

## Aggregate(s)

* GatewayAggregate
  * Manages the lifecycle and integrity of an integration gateway

## Value Objects

* GatewayId — Unique identifier for a gateway instance
* GatewayStatus — Enum for lifecycle state (Defined, Active, Disabled)
* GatewayRouteId — Reference to the routing configuration

## Domain Events

* GatewayCreatedEvent — Raised when a new gateway is defined
* GatewayActivatedEvent — Raised when the gateway is activated
* GatewayDisabledEvent — Raised when the gateway is disabled

## Specifications

* CanActivateSpecification — Only Defined gateways can be activated
* CanDisableSpecification — Only Active gateways can be disabled
* IsActiveSpecification — Checks if gateway is currently active

## Domain Services

* GatewayService — Domain operations for gateway management

## Errors

* MissingId — GatewayId is required
* MissingRouteId — GatewayRouteId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyActive — Gateway already in Active state
* AlreadyDisabled — Gateway already in Disabled state

## Invariants

* GatewayId must not be null/default
* GatewayRouteId must not be null/default
* GatewayStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* endpoint (gateway routes to endpoints)
* adapter (gateway may use adapters for protocol translation)

## Status

**S4 — Invariants + Specifications Complete**
