# Domain: EventEnvelope

## Classification

core-system

## Context

event

## Purpose

Defines the transport envelope that wraps domain events with correlation, causation, and routing metadata. This domain defines event envelope structure only and contains no event publication or transport logic.

## Core Responsibilities

* Wrapping domain events with correlation and causation identifiers
* Enforcing the standard envelope structure for all published events
* Managing the terminal lifecycle of an event envelope (Sealed -> Published -> Acknowledged)

## Aggregate(s)

* EventEnvelopeAggregate

  * Factory: Seal(id, metadata) creates a sealed envelope
  * Transitions: Publish(), Acknowledge()
  * Terminal lifecycle: Sealed -> Published -> Acknowledged

## Value Objects

* EventEnvelopeId — Unique validated Guid identifier for an event-envelope instance
* EventEnvelopeStatus — Enum: Sealed, Published, Acknowledged
* EventEnvelopeMetadata — Record struct with CorrelationId (Guid), CausationId (Guid), EventName (string)

## Domain Events

* EventEnvelopeSealedEvent — Raised when a new event envelope is sealed with metadata
* EventEnvelopePublishedEvent — Raised when an event envelope transitions to Published
* EventEnvelopeAcknowledgedEvent — Raised when an event envelope transitions to Acknowledged

## Specifications

* CanPublishSpecification — Validates status == Sealed before publishing
* CanAcknowledgeSpecification — Validates status == Published before acknowledging

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Errors

* MissingId — EventEnvelopeId is required and must not be empty
* MissingMetadata — Event envelope must include valid metadata
* InvalidStateTransition(status, action) — Cannot perform action in current status

## Invariants

* EventEnvelopeId must not be empty/default
* EventEnvelopeMetadata must not be default
* Status must be a defined enum value
* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

TERMINAL: Sealed -> Published -> Acknowledged

## Boundary

This domain defines event envelope structure only and contains no event publication or transport logic.

## Notes

Core-system must remain minimal, pure, and reusable.
