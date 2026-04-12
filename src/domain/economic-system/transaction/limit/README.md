# Domain: Limit

## Classification
economic-system

## Context
transaction

## Purpose
Enforces constraints on transactions — caps and thresholds that must be validated before execution. Limits track cumulative utilization and block transactions that would exceed defined thresholds.

## Core Responsibilities
- Defining transaction limits per account (caps, thresholds)
- Enforcing constraints before execution
- Tracking cumulative utilization across checks
- Recording limit checks and exceedances

## Aggregate(s)
- **LimitAggregate**
  - Event-sourced, sealed. Manages limit definition, checking, and utilization tracking
  - Invariants: Threshold > 0; CurrentUtilization >= 0; only Active limits can be checked; exceeding transitions to Exceeded status

## Entities
None

## Value Objects
- **LimitId** — Typed Guid wrapper for unique limit identity
- **LimitType** — Enum: PerTransaction, DailyVolume, DailyCount
- **LimitStatus** — Enum: Active, Exceeded

## Domain Events
- **LimitDefinedEvent** — Limit created for account with type, threshold, currency
- **LimitCheckedEvent** — Limit validated, transaction within bounds (T3 satisfied)
- **LimitExceededEvent** — Transaction would exceed threshold (blocks execution)

## Specifications
- **IsWithinLimitSpecification** — Status=Active AND (CurrentUtilization + transactionAmount <= Threshold)

## Domain Services
- **LimitEnforcementService** — Evaluates transaction against limit: validates Active status AND (CurrentUtilization + transactionAmount <= Threshold)

## Invariants (CRITICAL)
- Cannot execute beyond limit — exceeding throws and blocks
- Must validate before execution (T3)
- Threshold must be positive
- Only Active limits can be checked
- Utilization is cumulative across checks

## Policy Dependencies
- T3: Transaction must validate limits before execution

## Integration Points
- **transaction** — Limits validated before transaction completion; limit check must precede TransactionAggregate.Complete()
- **account** (capital context) — Limits reference constrained account via AccountId

## Lifecycle
```
Define() -> Active (zero utilization)
  Check() (pass) -> Active (utilization updated, LimitCheckedEvent)
  Check() (fail) -> Exceeded (LimitExceededEvent, exception thrown)
```

## Notes
- When Check passes: CurrentUtilization updated, LimitCheckedEvent raised
- When Check fails: LimitExceededEvent raised, exception thrown, Status changes to Exceeded
- Cross-domain references (AccountId, TransactionId) use raw Guid to avoid coupling
- All error methods are strongly typed via static LimitErrors class
