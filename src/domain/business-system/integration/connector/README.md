# Domain: Connector

## Classification

business-system

## Context

integration

## Purpose

Manages system connections for integration. A connector represents a logical connection to an external system, tracked through definition, connection, and disconnection states.

## Core Responsibilities

* Define connector identity and target reference
* Track connector lifecycle state (Defined → Connected → Disconnected)
* Enforce connection configuration before connecting
* Ensure disconnection only follows connection

## Aggregate(s)

* ConnectorAggregate
  * Manages the lifecycle and integrity of a system connector

## Value Objects

* ConnectorId — Unique identifier for a connector instance
* ConnectorStatus — Enum for lifecycle state (Defined, Connected, Disconnected)
* ConnectorTargetId — Reference to the target system

## Domain Events

* ConnectorCreatedEvent — Raised when a new connector is defined
* ConnectorConnectedEvent — Raised when the connector is connected
* ConnectorDisconnectedEvent — Raised when the connector is disconnected

## Specifications

* CanConnectSpecification — Only Defined connectors can connect
* CanDisconnectSpecification — Only Connected connectors can disconnect
* IsConnectedSpecification — Checks if connector is currently connected

## Domain Services

* ConnectorService — Domain operations for connector management

## Errors

* MissingId — ConnectorId is required
* MissingTargetId — ConnectorTargetId is required
* InvalidStateTransition — Attempted transition not allowed from current status
* AlreadyConnected — Connector already in Connected state
* AlreadyDisconnected — Connector already in Disconnected state

## Invariants

* ConnectorId must not be null/default
* ConnectorTargetId must not be null/default
* ConnectorStatus must be a defined enum value
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* adapter (connector may use adapter for protocol translation)
* endpoint (connector targets endpoints)

## Status

**S4 — Invariants + Specifications Complete**
