# Domain: Endpoint

## Classification

business-system

## Context

integration

## Purpose

Manages integration endpoint definitions — the addressable targets for sending or receiving integration messages. Tracks endpoint availability as a deterministic state (not network-driven). Requires a valid endpoint definition with URI, method, and protocol metadata.

## Core Responsibilities

* Define endpoint identity and definition
* Track endpoint lifecycle state (Defined → Available ↔ Unavailable)
* Enforce definition-before-availability guard
* Availability is state-driven, not network-driven (deterministic)

## Aggregate(s)

* EndpointAggregate
  * Manages the lifecycle and definition of an integration endpoint

## Entities

* EndpointDefinition — Endpoint metadata (URI, Method, Protocol)

## Value Objects

* EndpointId — Unique identifier for an endpoint instance
* EndpointStatus — Enum for lifecycle state (Defined, Available, Unavailable)
* EndpointUri — Endpoint location reference (validated non-empty string)

## Domain Events

* EndpointCreatedEvent — Raised when a new endpoint is defined
* EndpointMarkedAvailableEvent — Raised when endpoint is marked available
* EndpointMarkedUnavailableEvent — Raised when endpoint is marked unavailable

## Specifications

* CanMarkAvailableSpecification — Only Defined or Unavailable endpoints can be marked available
* CanMarkUnavailableSpecification — Only Available endpoints can be marked unavailable
* IsAvailableSpecification — Checks if endpoint is currently available

## Domain Services

* EndpointService — Reserved for cross-aggregate coordination

## Errors

* MissingId — EndpointId is required
* MissingDefinition — EndpointDefinition is required
* InvalidStateTransition — Invalid state transition attempted
* AlreadyAvailable — Endpoint already available
* AlreadyUnavailable — Endpoint already unavailable

## Invariants

* EndpointId must not be null/default
* EndpointDefinition must not be null
* EndpointStatus must be a defined enum value
* Availability toggle: Available ↔ Unavailable (bidirectional from those states)
* State transitions enforced by specifications

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* adapter (endpoints are served by adapters)
* client (clients connect to endpoints)

## Status

**S4 — Invariants + Specifications Complete**
