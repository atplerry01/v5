# Domain: Authority

## Classification
structural-system

## Context
cluster

## Domain Responsibility
This domain defines governance authority lifecycle within a cluster and contains no orchestration or execution logic.

## Aggregate

* **AuthorityAggregate** -- Root aggregate representing governance authority within a cluster.
  * Private constructor; created via `Establish(AuthorityId, AuthorityDescriptor)` factory method.
  * State transitions via `Activate()` and `Revoke()` methods.
  * Event-sourced: all state derived from applied events.
  * Enforces invariants after every state change.
  * Supports optimistic concurrency via `Version` property.

## State Model

```
Establish() -> Established --Activate()--> Active --Revoke()--> Revoked (terminal)
```

## Value Objects

* **AuthorityId** -- Deterministic identifier (validated non-empty Guid).
* **AuthorityStatus** -- Enum: `Established`, `Active`, `Revoked`.
* **AuthorityDescriptor** -- Record struct with `ClusterReference` (Guid, non-empty) and `AuthorityName` (string, non-empty).

## Events

* **AuthorityEstablishedEvent(AuthorityId, AuthorityDescriptor)** -- Authority created with initial descriptor.
* **AuthorityActivatedEvent(AuthorityId)** -- Authority transitioned to Active.
* **AuthorityRevokedEvent(AuthorityId)** -- Authority transitioned to Revoked (terminal).

## Invariants

* AuthorityId must not be null/default.
* AuthorityDescriptor must not be default.
* AuthorityStatus must be a defined enum value.
* State transitions enforced by specifications (Established allows Activate; Active allows Revoke).

## Specifications

* **CanActivateSpecification** -- Validates that status is Established before activating.
* **CanRevokeSpecification** -- Validates that status is Active before revoking.

## Errors

* **MissingId** -- AuthorityId is required.
* **MissingDescriptor** -- AuthorityDescriptor is required.
* **InvalidStateTransition** -- Guard for illegal status transitions (includes current status and attempted action).

## Domain Services

* **AuthorityService** -- Reserved for cross-aggregate coordination within authority context.

## Status

**S4 -- Invariants + Specifications Complete**
