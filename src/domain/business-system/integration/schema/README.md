# Domain: Schema

## Classification

business-system

## Context

integration

## Boundary Statement

This domain defines coordination contracts only and contains no execution logic.

## Lifecycle Pattern

**TERMINAL** — Draft → Published → Finalized. Finalized schemas are immutable and cannot be modified.

## Domain Responsibility

Models structural contract definitions for integration data formats. A schema defines the expected structure of exchanged data, progressing from draft through publication to finalization. Once finalized, a schema is immutable.

## Aggregate

* **SchemaAggregate** — Root aggregate managing schema lifecycle.
  * Private constructor; created via `Create(SchemaId, SchemaDefinitionId)` factory method.
  * State transitions via `Publish()` and `Finalize()` methods.
  * Event-sourced with optimistic concurrency via `Version`.

## State Model

```
Draft ──Publish()──> Published ──Finalize()──> Finalized (terminal, immutable)
```

## Value Objects

* **SchemaId** — Deterministic identifier (validated non-empty Guid).
* **SchemaStatus** — Enum: `Draft`, `Published`, `Finalized`.
* **SchemaDefinitionId** — Reference to the structural definition (validated non-empty Guid).

## Events

* **SchemaCreatedEvent** — Raised when schema is drafted (status: Draft).
* **SchemaPublishedEvent** — Raised when schema is published.
* **SchemaFinalizedEvent** — Raised when schema is finalized (immutable).

## Invariants

* SchemaId must not be null/default.
* SchemaDefinitionId must not be null/default.
* SchemaStatus must be a defined enum value.
* Finalized schemas are immutable — no further transitions allowed.

## Specifications

* **CanPublishSpecification** — Only Draft schemas can be published.
* **CanFinalizeSpecification** — Only Published schemas can be finalized.
* **IsFinalizedSpecification** — Checks if schema is immutable.

## Errors

* **MissingId** — SchemaId is required.
* **MissingDefinitionId** — SchemaDefinitionId is required.
* **AlreadyPublished** — Schema already published.
* **AlreadyFinalized** — Schema already finalized and immutable.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Status

**S4 — Invariants + Specifications Complete**
