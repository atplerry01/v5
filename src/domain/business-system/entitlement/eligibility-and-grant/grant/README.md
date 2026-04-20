# Grant

**Classification:** business-system
**Context:** entitlement
**Domain-Group:** eligibility-and-grant
**Domain:** grant
**Namespace:** `Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Grant`
**DomainRoute:** `(business, entitlement, grant)`

## Responsibility
Business entitlement grant truth. Owns the identity, subject/target binding, scope, and lifecycle (pending → active → revoked/expired) of a business entitlement grant. Does not own access-token/session mechanics or runtime access-control enforcement.

## Aggregate
- `GrantAggregate` — root aggregate. States: `Pending`, `Active`, `Revoked`, `Expired`. Once terminal (Revoked/Expired), cannot be reactivated.

## Value Objects
- `GrantId` — unique identifier
- `GrantSubjectRef` — local reference to the subject receiving the grant
- `GrantTargetRef` — local reference to the entitlement target (right/entitlement kind)
- `GrantScope` — business-meaning scope label for the grant
- `GrantStatus` — `Pending | Active | Revoked | Expired`
- `GrantExpiry` — optional; wraps a `DateTimeOffset` expiry moment

## Events
- `GrantCreatedEvent` — emitted on aggregate creation
- `GrantActivatedEvent` — emitted when a pending grant is activated
- `GrantRevokedEvent` — emitted when an active or pending grant is revoked
- `GrantExpiredEvent` — emitted when a grant with an expiry crosses its expiry moment

## Specifications
- `CanActivateSpecification` — activation allowed only from `Pending`
- `CanRevokeSpecification` — revocation allowed from `Pending` or `Active`
- `CanExpireSpecification` — expiry allowed only from non-terminal states and only when an `Expiry` is set and reached

## Errors (`GrantErrors` → `GrantDomainException`)
- `MissingId`, `MissingSubject`, `MissingTarget`
- `InvalidStateTransition`, `AlreadyTerminal`, `ExpiryInPast`

## Invariants
- Non-empty `GrantId`, `GrantSubjectRef`, `GrantTargetRef` at all times
- Status transitions are linear and terminal: `Pending → Active`, `{Pending, Active} → Revoked`, `{Pending, Active} → Expired`
- Expiry, if provided, must not already be in the past at the moment of creation or activation
- No reactivation after a terminal state

## Determinism
- No `Guid.NewGuid`, no `DateTime.UtcNow`/`DateTimeOffset.UtcNow`; callers pass ids and `now` timestamps explicitly to `Create`, `Activate`, `Revoke`, `Expire`.

## Status
S4 — E1 domain model implemented. Aggregate + VOs + events + specs + errors. No service or entity required at this stage.
