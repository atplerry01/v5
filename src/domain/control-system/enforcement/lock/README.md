# Domain: Lock

## Classification
economic-system

## Context
enforcement

## Purpose
Represents a full lock — complete suspension of a subject's ability to transact within a given scope. Locks are typically issued in response to Critical-severity escalation decisions.

## Aggregate(s)
- **LockAggregate** — Event-sourced. Lifecycle: Locked -> Unlocked (terminal).
  - Invariants: LockId cannot be empty; must reference a subject; cannot unlock twice.

## Value Objects
- **LockId** — Typed Guid wrapper
- **SubjectId** — Typed Guid wrapper
- **LockStatus** — Enum: Locked, Unlocked
- **LockScope** — Enum: Capital, Account, System
- **Reason** — Non-empty string wrapper

## Domain Events
- **SystemLockedEvent** — Lock applied to subject
- **SystemUnlockedEvent** — Lock released (terminal)

## Specifications
- **CanUnlockSpecification** — Status == Locked

## Invariants (CRITICAL)
- Every lock must reference a subject
- Every lock must include a reason
- Only Locked subjects can be unlocked
- Unlocked locks are terminal (a new lock aggregate must be created for a subsequent lock)

## Integration Points
- **sanction** — A sanction of type Lock produces a LockAggregate at enforcement time
- **escalation** — Critical-level escalation triggers system lock via policy, not via domain

## Lifecycle
```
Lock() -> Locked (requires subject, scope, reason)
  Unlock() -> Unlocked (terminal, requires reason)
```
