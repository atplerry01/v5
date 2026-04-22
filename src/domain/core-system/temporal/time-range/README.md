# core-system / temporal / time-range

**Classification:** core-system  
**Context:** temporal  
**Domain:** time-range

## Purpose
A closed time interval with explicit start and end — both bounds are required. Distinguished from `time-window` (which allows an open end) by its closed invariant.

## Scope
- `TimeRange` value object: start (DateTimeOffset), end (DateTimeOffset)
- Derived properties: duration, midpoint
- Structural queries: overlap, contains, precedes, follows (pure functional)

## Does Not Own
- Open-ended intervals (→ time-window)
- Business-specific range semantics (→ consuming domain)

## Inputs
- start (DateTimeOffset), end (DateTimeOffset) — both required, start < end

## Outputs
- `TimeRange` value object — no domain events

## Invariants
- start strictly precedes end — construction fails if violated
- Immutable once constructed

## Dependencies
- None
