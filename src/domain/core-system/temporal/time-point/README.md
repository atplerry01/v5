# core-system / temporal / time-point

**Classification:** core-system  
**Context:** temporal  
**Domain:** time-point

## Purpose
A discrete, named instant in time. Represents a single moment with an optional label identifying its semantic role (e.g., "issued-at", "expires-at") without encoding business meaning.

## Scope
- `TimePoint` value object: timestamp (DateTimeOffset), label (optional, opaque string)
- Comparison: before / after / equals (by timestamp only)

## Does Not Own
- Time generation or current time (→ engine layer via IClock)
- Business-specific point semantics (→ consuming domain)

## Inputs
- timestamp (DateTimeOffset), label (string, optional)

## Outputs
- `TimePoint` value object — no domain events

## Invariants
- Immutable once constructed
- Label is informational only; comparison uses timestamp exclusively

## Dependencies
- None
