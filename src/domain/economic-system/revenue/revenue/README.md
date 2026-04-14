# Domain: Revenue

## Classification
economic-system

## Context
revenue

## Purpose
Recognizes earned revenue based on contract and pricing. A revenue record represents the system acknowledgment that value has been earned per contract terms.

## Core Responsibilities
- Recognizing revenue from contracts
- Tracking revenue lifecycle (Recognized -> Distributed)
- Enforcing contract reference requirement (R1)

## Aggregate(s)
- **RevenueAggregate**
  - Event-sourced, sealed. Manages revenue recognition and distribution marking
  - Invariants: Amount must be >= 0; must reference a contract (ContractId non-empty — R1); only Recognized revenue can be distributed; cannot distribute twice

## Entities
None

## Value Objects
- **RevenueId** — Typed Guid wrapper with From() factory for unique revenue identity
- **RevenueStatus** — Enum: Recognized, Distributed

## Domain Events
- **RevenueRecordedEvent** — SPV revenue recorded from external income (captures RevenueId, ContractId, Amount, Currency)

```text
ECONOMIC FLOW

1. RevenueAggregate → RevenueRecordedEvent
2. DistributionAggregate → DistributionCreatedEvent
3. PayoutAggregate → PayoutExecutedEvent

Execution of vault mutation is handled outside the domain.
```

## Specifications
- **CanDistributeSpecification** — Status == Recognized

## Domain Services
- **RevenueTraceService** — Validates contract reference is non-empty; enforces R1 (revenue cannot exist without contract)

## Invariants (CRITICAL)
- Must reference a contract (non-empty ContractId) — R1
- Amount must be positive at recognition
- Amount must be non-negative (invariant)
- Only Recognized revenue can be distributed
- Cannot distribute twice

## Policy Dependencies
- R1: Revenue cannot exist without a contract

## Integration Points
- **contract** — Revenue recognition requires contract reference (R1)
- **distribution** — Revenue marked as distributed triggers distribution domain

## Lifecycle
```
Recognize() -> Recognized (requires ContractId)
  MarkDistributed() -> Distributed (terminal)
```

## Notes
- All error methods are strongly typed via static RevenueErrors class
