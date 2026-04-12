# Domain: Allocation

## Classification
economic-system

## Context
capital

## Purpose
Tracks directed capital movement from a source account to a specific target. Provides traceability for every unit of capital that leaves an account.

## Core Responsibilities
- Recording capital allocations with source and target references
- Tracking allocation lifecycle: Pending -> Completed or Released
- Ensuring all allocations are traceable to a source account

## Aggregate(s)
- **CapitalAllocationAggregate**
  - Event-sourced, sealed. Records capital allocation from source to target with amount and currency
  - Invariants: Amount must be positive; SourceAccountId must be non-empty; state transitions are unidirectional (Pending -> Completed or Released, mutually exclusive)

## Entities
None

## Value Objects
- **AllocationId** — Typed Guid wrapper for unique allocation identity
- **TargetId** — Typed Guid wrapper for allocation target reference
- **AllocationStatus** — Enum: Pending, Completed, Released

## Domain Events
- **AllocationCreatedEvent** — Allocation created with source, target, amount, currency
- **AllocationCompletedEvent** — Allocation finalized
- **AllocationReleasedEvent** — Allocation reversed, capital returned to source

## Specifications
- **CanCompleteSpecification** — Status == Pending
- **CanReleaseSpecification** — Status == Pending

## Domain Services
- **AllocationTraceService** — Validates that allocations are traceable to valid source and target (SourceAccountId and TargetId non-empty)

## Invariants (CRITICAL)
- Amount must be positive
- SourceAccountId must be non-empty
- Can only transition from Pending to Completed or Released (mutually exclusive)
- Cannot complete a released allocation
- Cannot release a completed allocation

## Policy Dependencies
- Traceability: every allocation must reference a valid source and target

## Integration Points
- **account** — Allocations reference source account via Guid AccountId

## Lifecycle
```
Allocate() -> Pending
  Complete() -> Completed (terminal)
  OR
  Release() -> Released (terminal)
```

## Notes
- Cross-domain references (SourceAccountId) use raw Guid to avoid coupling
- All error methods are strongly typed via static AllocationErrors class
