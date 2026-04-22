# core-system / temporal / time-window

**Classification:** core-system  
**Context:** temporal  
**Domain:** time-window

## Purpose
Immutable bounded time interval: a start timestamp and an optional end timestamp. Represents any finite or open-ended duration used across the system.

## Scope
- TimeWindow value object: start, end (optional), duration, overlap detection, containment checks
- Pure functional — no side effects, no state transitions

## Does Not Own
- Clock access or current time (→ engine layer via IClock)
- Scheduling or recurrence rules (→ runtime)
- Business-specific validity semantics (→ consuming domain)

## Inputs
- start (DateTimeOffset), end (DateTimeOffset, optional)

## Outputs
- `TimeWindow` value object — no domain events at primitive level

## Invariants
- start must precede end when end is present
- TimeWindow is immutable; adjustments return a new instance
- Open-ended windows (no end) represent indefinite validity

## Dependencies
- None
