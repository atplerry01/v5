# core-system / ordering / sequence

**Classification:** core-system  
**Context:** ordering  
**Domain:** sequence

## Purpose
Monotonic sequence number primitive. Represents an ordered position within any ordered set — used by event streams, timeline points, and any domain needing stable, comparable position values.

## Scope
- Sequence value object: value (long), comparison, equality
- SequenceRange value object: start sequence, end sequence, count

## Does Not Own
- Sequence generation or incrementing (→ engine layer)
- Domain-specific ordering rules (→ consuming domain)

## Inputs
- value (long, non-negative)

## Outputs
- `Sequence` value object — no domain events at primitive level

## Invariants
- Sequence value is non-negative
- Sequence is immutable once constructed
- Sequence comparison is by value only (no semantic meaning attached)

## Dependencies
- None
