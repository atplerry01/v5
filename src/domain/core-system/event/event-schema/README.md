# Domain: EventSchema

## Classification

core-system

## Context

event

## Purpose

Defines the schema validation rules for event payloads. Provides structural contracts that events must conform to for cross-system compatibility.

## Core Responsibilities

* Defines structural validation rules for event payloads
* Enforces cross-system compatibility contracts for events
* Manages schema versioning and evolution rules

## Aggregate(s)

* EventSchemaAggregate

  * Manages the lifecycle and integrity of an event schema

## Value Objects

* EventSchemaId — Unique identifier for a event-schema instance

## Domain Events

* EventSchemaCreatedEvent — Raised when a new event-schema is created
* EventSchemaUpdatedEvent — Raised when event-schema metadata is updated
* EventSchemaStateChangedEvent — Raised when event-schema lifecycle state transitions

## Specifications

* EventSchemaSpecification — Validates event-schema structure and completeness

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

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
