# Domain: Charge

## Classification
economic-system

## Context
transaction

## Purpose
Applies pricing and fees to transactions. Charges are deterministic (calculated from base amount + type) and traceable (always linked to a transaction). Lifecycle: Calculate -> Apply.

## Core Responsibilities
- Calculating fees (fixed or percentage-based)
- Attaching charges to transactions for traceability
- Ensuring charges are deterministic and reproducible

## Aggregate(s)
- **ChargeAggregate**
  - Event-sourced, sealed. Manages charge calculation and application lifecycle
  - Invariants: ChargeAmount >= 0; BaseAmount > 0; must reference a transaction (TransactionId non-empty); only Calculated charges can be Applied; cannot apply twice

## Entities
None

## Value Objects
- **ChargeId** — Typed Guid wrapper for unique charge identity
- **ChargeType** — Enum: Fixed, Percentage
- **ChargeStatus** — Enum: Calculated, Applied

## Domain Events
- **ChargeCalculatedEvent** — Fee calculated for a transaction (captures type, base amount, charge amount, currency)
- **ChargeAppliedEvent** — Fee applied to the transaction (T4 fulfilled)

## Specifications
- **CanApplySpecification** — Status == Calculated

## Domain Services
- **ChargeCalculationService** — Calculates fees deterministically: CalculateFixedCharge (validates > 0, returns same amount) and CalculatePercentageCharge (validates base > 0, calculates rounded percentage with banker's rounding)

## Invariants (CRITICAL)
- Charges must be deterministic — same inputs always produce same charge amount
- Charges must be traceable — every charge references a transaction
- Base amount must be positive
- Charge amount must be non-negative
- Only Calculated charges can be applied
- Cannot apply twice

## Policy Dependencies
- T4: Charges must be applied before journal creation
- Deterministic calculation enforcement

## Integration Points
- **transaction** — Charges reference transactions via TransactionId; charges must be applied before transaction completion

## Lifecycle
```
Calculate() -> Calculated
  ApplyCharge() -> Applied (terminal)
```

## Notes
- Percentage calculations use MidpointRounding.ToEven (banker's rounding) with 2 decimal precision
- Cross-domain references (TransactionId) use raw Guid to avoid coupling
- All error methods are strongly typed via static ChargeErrors class
