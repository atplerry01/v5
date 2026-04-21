# Domain: EventStream

## Classification

core-system

## Context

event

## Purpose

Defines the logical grouping and ordering of events within a stream. Provides the foundational structure for event stream identity and lifecycle management.

## Core Responsibilities

* Define the structural representation of event streams
* Enforce stream lifecycle transitions (Open → Sealed → Archived)
* Emit domain events on stream lifecycle transitions

## Aggregate(s)

* EventStreamAggregate
  * Represents the root entity for an event stream, encapsulating its identity, descriptor, and lifecycle rules

## Value Objects

* EventStreamId — Unique identifier for an event stream instance
* EventStreamStatus — Lifecycle state (Open, Sealed, Archived)
* StreamDescriptor — Record struct with StreamName and AggregateType

## Domain Events

* EventStreamOpenedEvent — Raised when a new event stream is opened
* EventStreamSealedEvent — Raised when an event stream is sealed
* EventStreamArchivedEvent — Raised when an event stream is archived

## Specifications

* CanSealSpecification — Guards transition from Open to Sealed
* CanArchiveSpecification — Guards transition from Sealed to Archived

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Invariants

* Event stream must have a valid identity (non-empty EventStreamId)
* Event stream must have a valid descriptor (non-default StreamDescriptor)
* Status must be a defined enum value
* Cannot be modified after archival (TERMINAL)

## Boundary Statement

This domain defines event stream structure only and contains no event storage or replay logic.

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Lifecycle

Open → Sealed → Archived

**Pattern: TERMINAL** — Once archived, an event stream cannot be reopened or modified.

## Status

**S4 — Invariants + Specifications Complete**
