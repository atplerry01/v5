# Domain: EventCatalog

## Classification

core-system

## Context

event

## Purpose

Defines the catalog of all event types in the system. Provides the registry for event type discovery and metadata.

## Core Responsibilities

* Maintains the authoritative registry of all known event types
* Supports discovery and lookup of event type metadata
* Enforces uniqueness and consistency of event type registrations

## Aggregate(s)

* EventCatalogAggregate

  * Manages the lifecycle and integrity of the event catalog registry

## Entities

* None

## Value Objects

* EventCatalogId — Unique identifier for a event-catalog instance

## Domain Events

* EventCatalogCreatedEvent — Raised when a new event-catalog is created
* EventCatalogUpdatedEvent — Raised when event-catalog metadata is updated
* EventCatalogStateChangedEvent — Raised when event-catalog lifecycle state transitions

## Specifications

* EventCatalogSpecification — Validates event-catalog structure and completeness

## Domain Services

* EventCatalogService — Domain operations for event-catalog management

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Created -> Active -> Updated -> Deprecated

## Notes

Core-system must remain minimal, pure, and reusable.
