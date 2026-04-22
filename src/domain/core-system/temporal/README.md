# core-system / temporal

**Classification:** core-system  
**Context:** temporal

## Purpose
Temporal primitives: immutable structural expressions of time — bounded intervals, discrete points, ranges, and validity periods. No scheduling, no state, no clock access.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `time-window` | Bounded interval: start + optional end |
| `time-point` | A discrete, named instant in time |
| `time-range` | A closed interval with explicit start and end |
| `effective-period` | A validity window declaring when a value is in effect |

## Removed (violations)
- `clock` — infrastructure abstraction, belongs in engine layer
- `ordering` — promoted to own context (`core-system/ordering`)
- `schedule-reference` — scheduling semantics, not a primitive
- `temporal-state` — state transitions, belongs in engines
- `timeline` — lifecycle/progression semantics, not a primitive

## Does Not Own
- Clock or time generation (→ `IClock` in engine layer)
- Scheduling or recurrence (→ runtime)
- State machine transitions (→ engines)

## Invariants
- All values are immutable
- No behavior beyond structural validation

## Dependencies
- None
