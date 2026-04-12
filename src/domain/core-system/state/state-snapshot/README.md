# Domain: StateSnapshot

## Classification

core-system

## Context

state

## Purpose

Defines the foundational structure for state snapshots — point-in-time captures of aggregate state.

## Boundary

This domain defines state snapshot structure only and contains no state persistence or recovery logic.

## Core Responsibilities

* Define the structural contract for snapshot lifecycle and identity
* Enforce deterministic snapshot capture, verification, and expiration rules

## Aggregate(s)

* StateSnapshotAggregate

  * Factory: Capture(id, descriptor)
  * Transitions: Verify(), Expire()

## Entities

* None

## Value Objects

* StateSnapshotId — Validated Guid identifier for a state snapshot instance
* SnapshotStatus — Enum: Captured, Verified, Expired
* SnapshotDescriptor — Record struct with AggregateId (Guid), AggregateType (string), SequenceNumber (long)

## Domain Events

* StateSnapshotCapturedEvent(StateSnapshotId, SnapshotDescriptor) — Raised when a snapshot is captured
* StateSnapshotVerifiedEvent(StateSnapshotId) — Raised when a snapshot is verified
* StateSnapshotExpiredEvent(StateSnapshotId) — Raised when a snapshot expires

## Specifications

* CanVerifySpecification — status == Captured
* CanExpireSpecification — status == Verified

## Domain Services

* StateSnapshotService — Empty (no domain-level orchestration required)

## Errors

* MissingId — StateSnapshotId requires a non-empty Guid
* MissingDescriptor — StateSnapshot requires a valid SnapshotDescriptor
* InvalidStateTransition(status, action) — Returns InvalidOperationException

## Invariants

* SnapshotId must not be default
* Descriptor must not be default
* Status transitions are terminal: Captured -> Verified -> Expired

## Lifecycle

**TERMINAL**: Captured -> Verified -> Expired

## Policy Dependencies

* None (core-system must be policy-agnostic)

## Integration Points

* All systems (shared usage layer)

## Notes

Core-system must remain minimal, pure, and reusable.
