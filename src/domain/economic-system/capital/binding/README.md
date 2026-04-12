# Domain: Binding

## Classification
economic-system

## Context
capital

## Purpose
Manages the account-to-owner relationship. Every capital account must have exactly one active binding that declares who owns it and what type of ownership applies.

## Core Responsibilities
- Binding accounts to owners with ownership type classification
- Transferring ownership between parties
- Releasing bindings when accounts are closed
- Enforcing unique active ownership per account

## Aggregate(s)
- **BindingAggregate**
  - Event-sourced, sealed. Manages account-to-owner relationship with ownership type and lifecycle
  - Invariants: AccountId and OwnerId must not be empty for Active bindings; new owner must differ from current; cannot transfer after Release; cannot release after Transfer; can only operate on Active bindings

## Entities
None

## Value Objects
- **BindingId** — Typed Guid wrapper for unique binding identity
- **OwnershipType** — Enum: Individual, Joint, Corporate, Trust
- **BindingStatus** — Enum: Active, Transferred, Released

## Domain Events
- **CapitalBoundEvent** — Account bound to owner with ownership type
- **OwnershipTransferredEvent** — Ownership changed (captures previous and new owner)
- **BindingReleasedEvent** — Account-owner binding terminated

## Specifications
- **CanTransferSpecification** — Status == Active
- **CanReleaseSpecification** — Status == Active

## Domain Services
- **OwnershipValidationService** — Validates unique ownership per account; ensures no other Active bindings exist for same account

## Invariants (CRITICAL)
- AccountId and OwnerId must not be empty for Active bindings
- New owner must differ from current owner on transfer
- Cannot transfer a released binding
- Cannot release a transferred binding
- Each account has exactly one active binding

## Policy Dependencies
- Ownership uniqueness enforcement

## Integration Points
- **account** — Bindings associate with AccountId
- **asset** — Asset owner must match binding owner

## Lifecycle
```
Bind() -> Active
  TransferOwnership() -> Transferred (terminal)
  OR
  Release() -> Released (terminal)
```

## Notes
- Transfer and Release are mutually exclusive terminal states
- Cross-domain references (AccountId, OwnerId) use raw Guid to avoid coupling
- All error methods are strongly typed via static BindingErrors class
