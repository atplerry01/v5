# Domain: Reserve

## Classification
economic-system

## Context
capital

## Purpose
Manages time-bound capital holds. When capital is reserved on an account, a corresponding reserve record tracks the hold with an expiry time. Reserves must be explicitly released or expire automatically.

## Core Responsibilities
- Creating capital reservations with expiry semantics
- Tracking reserve lifecycle: Active -> Released or Expired
- Enforcing expiry-based release of held capital

## Aggregate(s)
- **ReserveAggregate**
  - Event-sourced, sealed. Manages time-bound capital holds with expiry-based state machine
  - Invariants: Amount must be positive; ExpiresAt must be after ReservedAt; only Active reserves can change state; Release and Expire are mutually exclusive terminal states

## Entities
None

## Value Objects
- **ReserveId** — Typed Guid wrapper with From() factory for unique reserve identity
- **ReserveStatus** — Enum: Active, Released, Expired

## Domain Events
- **ReserveCreatedEvent** — Time-bound capital hold created with expiry timestamp
- **ReserveReleasedEvent** — Reserve explicitly released before expiry
- **ReserveExpiredEvent** — Reserve expired by time

## Specifications
- **CanExpireSpecification** — Status == Active AND currentTime >= ExpiresAt
- **CanReleaseSpecification** — Status == Active

## Domain Services
- **ReserveExpiryService** — Manages time-based reserve expiration; attempts expiry via TryExpire(reserve, currentTime), returns success boolean

## Invariants (CRITICAL)
- Amount must be positive (> 0)
- ExpiresAt must be after ReservedAt
- Cannot release an expired reservation
- Cannot expire a released reservation
- Only Active reserves can change state

## Policy Dependencies
- Time-based constraints: ExpiresAt > ReservedAt enforced at creation
- IClock for deterministic time

## Integration Points
- **account** — Reserves hold against AccountId; AccountCapitalReservedEvent mirrors ReserveCreatedEvent

## Lifecycle
```
Create() -> Active
  Release() -> Released (terminal)
  OR
  Expire() -> Expired (terminal, time-triggered)
```

## Notes
- Cross-domain references (AccountId) use raw Guid to avoid coupling
- Time-based expiry uses IClock for deterministic behavior
- All error methods are strongly typed via static ReserveErrors class
