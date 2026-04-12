# Domain: Handshake

## Classification

business-system

## Context

integration

## Purpose

Defines the structure of integration handshakes — the initial connection establishment and capability negotiation records.

## Core Responsibilities

* Define the structural representation of integration handshakes
* Track handshake metadata and lifecycle state
* Emit domain events on handshake state changes

## Aggregate(s)

* HandshakeAggregate

  * Represents the root entity for an integration handshake, encapsulating its identity, metadata, and lifecycle state

## Entities

* None

## Value Objects

* HandshakeId — Unique identifier for a handshake instance

## Domain Events

* HandshakeCreatedEvent — Raised when a new handshake is created
* HandshakeUpdatedEvent — Raised when handshake metadata is updated
* HandshakeStateChangedEvent — Raised when handshake lifecycle state transitions

## Specifications

* HandshakeSpecification — Validates handshake structure and completeness

## Domain Services

* HandshakeService — Domain operations for handshake management

## Invariants

* Business entities must remain consistent
* Relationships must be valid
* No financial or execution logic allowed

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* structural-system
* economic-system (read-only relationship, no logic)
* decision-system

## Lifecycle

Created → Active → Updated → Inactive

## Notes

Business-system defines structure only. No financial, execution, or workflow logic allowed.
