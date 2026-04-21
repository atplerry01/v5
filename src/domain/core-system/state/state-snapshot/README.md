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

  * Inherits canonical `AggregateRoot`; created via `Capture(StateSnapshotId, SnapshotDescriptor)` factory.
  * Transitions: `Verify()`, `Expire()`.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via inherited `Version` property.

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

## Errors

* MissingId — StateSnapshotId requires a non-empty Guid.
* MissingDescriptor — StateSnapshot requires a valid SnapshotDescriptor.
* InvalidStateTransition — Guard for illegal status transitions.
* AlreadyInitialized — Factory invoked on an already-initialized aggregate.

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

## WHEN-NEEDED folders

* `entity/` — Omitted: this BC has no child entities; state is fully carried by the aggregate and its value objects.
* `service/` — Omitted: no cross-aggregate coordination is required within this BC.

## Notes

Core-system must remain minimal, pure, and reusable.
