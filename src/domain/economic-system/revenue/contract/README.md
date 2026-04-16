# Domain: Contract

## Classification
economic-system

## Context
revenue

## Purpose
Defines binding agreements that determine how revenue is generated and shared. A contract binds multiple parties with explicit revenue share rules that must total 100%.

## Core Responsibilities
- Defining revenue rules and binding parties
- Enforcing agreement lifecycle (Draft -> Active -> Terminated)
- Validating revenue share rules sum to 100%
- Ensuring at least 2 parties per contract

## Aggregate(s)
- **RevenueContractAggregate**
  - Event-sourced, sealed. Manages ContractParty entities
  - Invariants: Must have >= 2 parties; revenue shares must total exactly 100%; each share must be between 0 and 100%; end date must be after start date; cannot activate non-Draft; cannot re-terminate

## Entities
- **ContractParty** — Party bound to contract with revenue share percentage. Properties: PartyId (Guid), SharePercentage (decimal).

## Value Objects
- **RevenueContractId** — Typed Guid wrapper with From() factory for unique contract identity
- **ContractStatus** — Enum: Draft, Active, Terminated
- **ContractTerm** — Sealed record: StartDate, EndDate (EndDate must be > StartDate)
- **RevenueShareRule** — Sealed record: PartyId, SharePercentage (input type for contract creation)

## Domain Events
- **RevenueContractCreatedEvent** — Agreement drafted with parties and share rules
- **RevenueContractActivatedEvent** — Contract activated for revenue generation
- **RevenueContractTerminatedEvent** — Contract terminated with reason

## Specifications
- **IsActiveSpecification** — Status == Active

## Domain Services
- **ContractValidationService** — IsWithinTerm (Active AND currentTime between StartDate/EndDate); ValidateShareRules (>= 2 parties AND total shares = 100%)

## Invariants (CRITICAL)
- Must have at least 2 parties
- Revenue share must total exactly 100%
- Contract must be Active to generate revenue
- End date must be after start date
- Each party share must be between 0 and 100%
- Cannot activate non-Draft contracts
- Cannot re-terminate a terminated contract

## Policy Dependencies
- Share rule enforcement (must total 100%)
- Term validation (end > start)

## Integration Points
- **pricing** — Pricing references contract for value determination.
- **revenue** — Revenue originates from an SPV in the canonical
  (SPV-based, single-shot) model; contracts define the share rules and
  party bindings consumed by downstream orchestration (distribution
  allocations), but a revenue record does NOT carry a ContractId as
  its origin.

## Lifecycle
```
CreateContract() -> Draft (parties assigned, share rules set)
  Activate() -> Active (can generate revenue, read-only after activation)
  Terminate() -> Terminated (terminal, no further operations)
```

## Notes
- All error methods are strongly typed via static ContractErrors class
- ContractParty uses internal factory Create() method
- RevenueShareRule is an input type used at creation, not stored directly
