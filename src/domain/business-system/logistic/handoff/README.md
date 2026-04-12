# Domain: Handoff

## Classification

business-system

## Context

logistic

## Purpose

Defines the transfer boundary between actors in a logistic process. Handoff represents the explicit, unambiguous transfer of responsibility from a source actor to a target actor.

## Boundary Statement

This domain defines logistic coordination contracts only and contains no execution logic.

## Core Responsibilities

* Define explicit transfer declarations between source and target actors
* Enforce that handoff is unambiguous (source != target)
* Maintain actor reference integrity and transfer reference tracking

## Aggregate(s)

* HandoffAggregate
  * Event-sourced root entity governing a handoff record
  * Tracks transfer state between two distinct actors
  * Enforces invariants: valid id, distinct source/target actors, valid transfer reference, valid status

## Entities

* None

## Value Objects

* HandoffId — Unique typed identifier for a handoff instance
* HandoffStatus — Lifecycle state (Created, Transferred)
* ActorReference — Reference to an actor (source or target) participating in the transfer
* TransferReference — Reference identifying the transfer being executed

## Domain Events

* HandoffCreatedEvent — Raised when a new handoff is created (carries HandoffId, SourceActor, TargetActor, TransferReference)
* HandoffTransferredEvent — Raised when handoff reaches terminal transferred state (carries HandoffId)
* HandoffStateChangedEvent — Raised when handoff lifecycle state transitions (carries previous and new status)
* HandoffUpdatedEvent — Raised when handoff metadata is updated

## Specifications

* HandoffSpecification — Validates handoff structure: id, source, target, transfer reference valid and source != target
* HasValidActorsSpecification — Validates that source and target actors are non-default and distinct

## Domain Services

* HandoffService — Reserved for domain operations that span beyond a single aggregate

## Invariants

* Must define a source actor (ActorReference != default)
* Must define a target actor (ActorReference != default)
* Source and target actors must be distinct (source != target)
* Must have a valid HandoffId
* Must have a valid TransferReference
* Transfer must be explicit, not ambiguous
* Cannot be transferred more than once (terminal state)
* Status must be a defined enum value

## Lifecycle Pattern

TERMINAL: Created -> Transferred

## Policy Dependencies

* Structural or governance rules may be governed by WHYCEPOLICY

## Integration Points

* None (self-contained transfer boundary)

## Notes

No handoff coordination logic. No delivery logic. No real-time updates. No external dependencies. Deterministic only.
