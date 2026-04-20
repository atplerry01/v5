# Domain: Clause

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models individual contractual provisions within an agreement. Each clause has a type classification and follows a lifecycle from draft through activation to supersession.

## Aggregate

* **ClauseAggregate** — Root aggregate representing an individual contractual provision.
  * Private constructor; created via `Create(ClauseId, ClauseType)` factory method.
  * State transitions via `Activate()` and `Supersede()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Draft ──Activate()──> Active ──Supersede()──> Superseded (terminal)
```

## Value Objects

* **ClauseId** — Deterministic identifier (validated non-empty Guid).
* **ClauseType** — Enum: `General`, `Termination`, `Liability`, `Confidentiality`, `Indemnity`.
* **ClauseStatus** — Enum: `Draft`, `Active`, `Superseded`.

## Events

* **ClauseCreatedEvent** — Raised when a new clause is created (status: Draft, includes ClauseType).
* **ClauseActivatedEvent** — Raised when clause is activated.
* **ClauseSupersededEvent** — Raised when clause is superseded by a newer version.

## Invariants

* ClauseId must not be null/default.
* ClauseType must be a defined enum value.
* ClauseStatus must be a defined enum value.
* State transitions enforced by specifications.

## Specifications

* **IsValidClauseSpecification** — Validates that clause id and type are well-formed.
* **CanActivateClauseSpecification** — Only Draft clauses can be activated.
* **CanSupersedeClauseSpecification** — Only Active clauses can be superseded.

## Errors

* **MissingId** — ClauseId is required.
* **InvalidClauseType** — ClauseType must be a valid defined value.
* **AlreadySuperseded** — Cannot act on a superseded clause.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## Domain Services

* **ClauseService** — Reserved for cross-aggregate coordination within clause context.

## Status

**S4 — Invariants + Specifications Complete**
