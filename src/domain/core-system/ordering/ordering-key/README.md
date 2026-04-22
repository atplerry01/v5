# core-system / ordering / ordering-key

**Classification:** core-system  
**Context:** ordering  
**Domain:** ordering-key

## Purpose
A stable, comparable key used to deterministically order entities or records within an ordered set. Where `sequence` expresses position by integer, `ordering-key` expresses position by a comparable structural value (e.g., composite sort key, lexicographic key).

## Scope
- `OrderingKey` value object: value (string or byte array), comparison
- Structural queries: compareTo, precedes, follows, equals — pure functional

## Does Not Own
- Key generation logic (→ engine layer)
- Business-specific ordering rules (→ consuming domain)

## Inputs
- key value (string, non-null, non-empty)

## Outputs
- `OrderingKey` value object — no domain events

## Invariants
- Value is immutable once constructed
- Comparison is lexicographic by value unless overridden structurally
- Empty key values are forbidden

## Dependencies
- None
