# core-system / temporal / effective-period

**Classification:** core-system  
**Context:** temporal  
**Domain:** effective-period

## Purpose
A validity window declaring when a value or rule is in effect. Combines an optional start and an optional end — both open, left-open, right-open, or fully bounded. Represents "this applies from X until Y" as a structural contract.

## Scope
- `EffectivePeriod` value object: effectiveFrom (DateTimeOffset, optional), effectiveTo (DateTimeOffset, optional)
- Structural queries: isActive(at: DateTimeOffset), hasStarted(at), hasExpired(at) — pure functional
- Fully unbounded (no from, no to) is valid and means "always effective"

## Does Not Own
- Business-specific validity rules (→ consuming domain)
- Time generation (→ engine layer)

## Inputs
- effectiveFrom (DateTimeOffset, optional), effectiveTo (DateTimeOffset, optional)

## Outputs
- `EffectivePeriod` value object — no domain events

## Invariants
- When both bounds are set, effectiveFrom strictly precedes effectiveTo
- Immutable once constructed

## Dependencies
- None
