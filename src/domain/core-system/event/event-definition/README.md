# Domain: EventDefinition

## Classification

core-system

## Context

event

## Purpose

Defines the structure and schema of individual domain events. Provides the canonical shape of event payloads and their metadata.

## Core Responsibilities

* Defining the canonical schema and structure for event payloads
* Managing event definition lifecycle (Draft, Published, Deprecated)
* Providing structural metadata for event serialization and validation

## Aggregate(s)

* EventDefinitionAggregate

  * Factory: Register(id, schema) — creates a new event definition in Draft status
  * Transitions: Publish() (Draft -> Published), Deprecate() (Published -> Deprecated)

## Entities

* None

## Value Objects

* EventDefinitionId — Unique identifier for an event-definition instance (validated Guid)
* EventDefinitionStatus — Lifecycle state: Draft, Published, Deprecated
* EventSchema — Record struct with EventName (string) and SchemaVersion (int), both validated

## Domain Events

* EventDefinitionRegisteredEvent — Raised when a new event definition is registered
* EventDefinitionPublishedEvent — Raised when an event definition is published
* EventDefinitionDeprecatedEvent — Raised when an event definition is deprecated

## Specifications

* CanPublishSpecification — Validates that status is Draft before publishing
* CanDeprecateSpecification — Validates that status is Published before deprecating

## Domain Services

* EventDefinitionService — Empty (no domain operations required)

## Invariants

* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

TERMINAL: Draft -> Published -> Deprecated

## Boundary

This domain defines event structure only and contains no event publication or transport logic.

## Notes

Core-system must remain minimal, pure, and reusable.
