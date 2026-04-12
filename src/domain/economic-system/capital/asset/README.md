# Domain: Asset

## Classification
economic-system

## Context
capital

## Purpose
Models valued assets tied to owners. Assets have a monetary value that can be revalued over time and eventually disposed of. Asset values contribute to the economic picture of capital ownership.

## Core Responsibilities
- Creating assets with initial valuation
- Revaluing assets (appreciation tracking)
- Disposing of assets (terminal state)
- Maintaining non-negative asset values

## Aggregate(s)
- **AssetAggregate**
  - Event-sourced, sealed. Manages asset value, currency, and status through creation, revaluation, and disposal
  - Invariants: Value must be >= 0; initial and revaluation values must be positive; OwnerId must be non-empty; cannot operate on Disposed assets

## Entities
None

## Value Objects
- **AssetId** — Typed Guid wrapper for unique asset identity
- **AssetStatus** — Enum: Active, Valued, Disposed

## Domain Events
- **AssetCreatedEvent** — Asset registered with initial value and owner
- **AssetValuedEvent** — Asset revalued (captures previous and new value)
- **AssetDisposedEvent** — Asset end of life (captures final value)

## Specifications
- **CanDisposeSpecification** — Status != Disposed
- **CanRevalueSpecification** — Status != Disposed

## Domain Services
- **AssetValuationService** — Calculates asset value appreciation; returns positive difference or zero if depreciation

## Invariants (CRITICAL)
- Value must always be non-negative
- Initial value must be positive
- Revaluation value must be positive
- OwnerId must be non-empty
- Cannot revalue or dispose a disposed asset

## Policy Dependencies
- Currency consistency enforcement on all operations

## Integration Points
- **binding** — Asset owner must match binding owner

## Lifecycle
```
Create() -> Active
  Revalue() -> Valued (repeatable)
Dispose() -> Disposed (terminal)
```

## Notes
- Cross-domain references (OwnerId) use raw Guid to avoid coupling
- All error methods are strongly typed via static AssetErrors class
