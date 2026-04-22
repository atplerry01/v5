# core-system / identifier / global-identifier

**Classification:** core-system  
**Context:** identifier  
**Domain:** global-identifier

## Purpose
System-wide unique, deterministic identifier. The canonical base type for all entity IDs, aggregate IDs, and cross-boundary references. Derived from SHA256 of a deterministic input set.

## Scope
- `GlobalIdentifier` value object: value (64-char lowercase hex string)
- Equality, formatting, and null-safety guards

## Does Not Own
- ID generation (→ `IIdGenerator` in engine layer)
- Domain-specific ID wrapping (each domain defines its own `*Id` value object that holds a `GlobalIdentifier`)

## Inputs
- Pre-computed SHA256 hex string (64 lowercase hex characters)

## Outputs
- `GlobalIdentifier` value object — no domain events

## Invariants
- Value is exactly 64 lowercase hex characters
- Immutable once constructed
- No `Guid.NewGuid()`, `Random`, or system clock in construction

## Dependencies
- None
