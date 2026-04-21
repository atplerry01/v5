# Domain: Signature

## Classification

business-system

## Context

agreement

## Domain Responsibility

Models the formal execution marks that bind parties to agreement terms. Tracks signature lifecycle from pending through signed or revoked states. A signature authenticates a party's commitment to the agreement.

## Aggregate

* **SignatureAggregate** — Root aggregate representing a formal execution mark binding a party to agreement terms.
  * Private constructor; created via `Create(SignatureId)` factory method.
  * State transitions via `Sign()` and `Revoke()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Pending ──Sign()──> Signed
Signed ──Revoke()──> Revoked (terminal)
```

## Value Objects

* **SignatureId** — Deterministic identifier (validated non-empty Guid).
* **SignatureStatus** — Enum: `Pending`, `Signed`, `Revoked`.

## Events

* **SignatureCreatedEvent** — Raised when a new signature is created (status: Pending).
* **SignatureSignedEvent** — Raised when signature is executed.
* **SignatureRevokedEvent** — Raised when signature is revoked.

## Invariants

* SignatureId must not be null/default.
* SignatureStatus must be a defined enum value.
* Cannot sign twice (enforced by CanSignSpecification: only Pending allows signing).
* Revocation only allowed after signing (enforced by CanRevokeSpecification: only Signed allows revocation).

## Specifications

* **CanSignSpecification** — Validates that status is Pending before signing.
* **CanRevokeSpecification** — Validates that status is Signed before revoking.

## Errors

* **MissingId** — SignatureId is required.
* **AlreadySigned** — Cannot sign an already-signed signature.
* **AlreadyRevoked** — Cannot transition an already-revoked signature.
* **InvalidStateTransition** — Generic guard for illegal status transitions.

## WHEN-NEEDED folders

- no `entity/` — aggregate has no child entities.
- no `service/` — aggregate has no cross-aggregate coordination logic.

## Status

**S4 — Invariants + Specifications Complete**
