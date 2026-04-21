# Domain: CommandEnvelope

## Classification

core-system

## Context

command

## Purpose

Defines the transport envelope that wraps commands with routing, correlation, and causation metadata. Provides the standard wrapper structure for command dispatch traceability.

**Boundary statement:** This domain defines command envelope structure only and contains no dispatch or transport logic.

## Core Responsibilities

* Wrapping commands with correlation and causation identifiers
* Providing routing metadata for command dispatch infrastructure
* Ensuring traceability of command origin and processing chain
* Enforcing terminal lifecycle progression (Sealed -> Dispatched -> Acknowledged)

## Aggregate(s)

* CommandEnvelopeAggregate
  * Factory: `Seal(id, metadata)` — creates a sealed envelope
  * Transitions: `Dispatch()`, `Acknowledge()`
  * Private constructor, event-sourced state via Apply methods
  * Exposes uncommitted events via `GetUncommittedEvents()`

## Value Objects

* CommandEnvelopeId — Validated Guid identifier, rejects empty
* CommandEnvelopeStatus — Enum: Sealed, Dispatched, Acknowledged
* EnvelopeMetadata — Record struct with CorrelationId (Guid), CausationId (Guid), CommandName (string); all validated non-empty

## Domain Events

* CommandEnvelopeSealedEvent(EnvelopeId, Metadata) — Raised when an envelope is sealed with metadata
* CommandEnvelopeDispatchedEvent(EnvelopeId) — Raised when an envelope is dispatched
* CommandEnvelopeAcknowledgedEvent(EnvelopeId) — Raised when an envelope is acknowledged

## Specifications

* CanDispatchSpecification — Satisfied when status is Sealed
* CanAcknowledgeSpecification — Satisfied when status is Dispatched

## Errors

* MissingId — CommandEnvelopeId is required and must not be empty
* MissingMetadata — Command envelope must include valid metadata
* InvalidStateTransition(status, action) — Cannot perform action in current status

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Invariants

* Id must not be default (empty Guid)
* Metadata must not be default (all fields validated)
* Status must be a defined enum value
* Must remain deterministic
* Must remain reusable across systems
* Must not contain business-specific rules

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

**TERMINAL:** Sealed -> Dispatched -> Acknowledged

No reverse transitions. Each state transition is one-way and irreversible.

## Notes

Core-system must remain minimal, pure, and reusable. This domain defines command envelope structure only and contains no dispatch or transport logic.
