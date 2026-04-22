# Domain: Sanction

## Classification
economic-system

## Context
enforcement

## Purpose
Represents system-imposed restrictions as first-class, event-sourced state. Sanctions are issued in response to escalation/policy decisions (never from domain logic) and enforced at runtime. A sanction has a lifecycle: Issued -> Active -> (Expired | Revoked).

## Core Responsibilities
- Recording sanction lifecycle events per subject
- Enforcing state transitions (Issued -> Active -> terminal)
- Preserving issuance context (scope, type, reason, effective period)

## Aggregate(s)
- **SanctionAggregate** — Event-sourced, sealed. Lifecycle per subject+sanction identity.
  - Invariants: SanctionId cannot be empty; must reference a subject; cannot double-activate; cannot revoke or expire terminal sanctions.

## Value Objects
- **SanctionId** — Typed Guid wrapper
- **SubjectId** — Typed Guid wrapper referencing the sanctioned subject
- **SanctionStatus** — Enum: Issued, Active, Expired, Revoked
- **SanctionType** — Enum: Restriction, Lock
- **SanctionScope** — Enum: Capital, Account, System
- **Reason** — Non-empty string wrapper
- **EffectivePeriod** — EffectiveAt (required) + ExpiresAt (optional)

## Domain Events
- **SanctionIssuedEvent** — Sanction created
- **SanctionActivatedEvent** — Sanction activated (effective)
- **SanctionExpiredEvent** — Sanction reached expiry
- **SanctionRevokedEvent** — Sanction revoked with reason

## Specifications
- **CanActivateSpecification** — Status == Issued
- **CanRevokeSpecification** — Status in {Issued, Active}
- **CanExpireSpecification** — Status == Active

## Invariants (CRITICAL)
- Every sanction must reference a subject
- Every sanction must include a reason
- Only Issued sanctions can be activated
- Only Active sanctions can be expired
- Revoked/Expired sanctions are terminal

## Integration Points
- **escalation** — Escalation decisions trigger sanction issuance (via policy, NOT domain)
- **violation** — Violations accumulate into escalations that eventually produce sanctions
- **capital** — Sanctioned subjects are freezed at the capital-account boundary via runtime bridge

## Lifecycle
```
Issue() -> Issued (requires subject, reason, scope, type, effective period)
  Activate() -> Active
    Revoke() -> Revoked (terminal)
    Expire() -> Expired  (terminal)
```

## Notes
- Domain does NOT decide WHEN sanctions are issued. Policy/escalation decides; domain records.
- Runtime enforces sanction effects across system boundaries. Domain only holds truth.
