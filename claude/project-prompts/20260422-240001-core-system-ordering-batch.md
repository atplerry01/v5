# TITLE
core-system / ordering — E1→EX Full Domain Batch

# CONTEXT

CLASSIFICATION: core-system
CONTEXT: ordering
DOMAIN GROUP: ordering
DOMAINS:
- sequence
- ordering-key
- ordering-rule

Optional batch description:
Canonical ordering primitives — deterministic, stable ordering of any sequenced data.
Sequence provides monotonic counters; OrderingKey provides stable, ordinal-comparison keys;
OrderingRule composes key + direction + optional tie-breaking into a complete, deterministic
ordering criterion.

# OBJECTIVE

Implement and maintain the `ordering` domain group inside `core-system` as pure semantic
primitives. All ordering must be deterministic, stable under replay, and free of ambiguous
or non-deterministic behavior.

Reference: `/claude/project-topics/v3/core-system.md` — sections 1–8, 11–22.

# CONSTRAINTS

1. Domain layer ZERO external dependencies.
2. No non-deterministic operations (no GUID, no clock, no RNG).
3. All types immutable: `readonly record struct` or `sealed record`.
4. OrderingRule tie-breaker chains must be acyclic and bounded (max depth 8).
5. Sequence must be non-negative and monotonically increasing via `Next()`.
6. OrderingKey comparison must be culture-invariant (`StringComparison.Ordinal`).
7. No aggregates with lifecycle, no commands, no events, no policy model.
8. core-system may not depend on any other classification.

# EXECUTION STEPS

## E1 — Domain Primitives

### sequence
- `Sequence` — non-negative monotonic counter; `Zero`, `Next()`, `Precedes`, `Follows`,
  comparison operators; `SequenceRange` for bounded windows.

### ordering-key
- `OrderingKey` — stable ordinal string key; `CompareTo`, `Precedes`, `Follows`,
  comparison operators.

### ordering-rule
- `OrderingDirection` enum: `Ascending`, `Descending`
- `OrderingRule` — binds an `OrderingKey` to a `Direction`; optional `TieBreaker`
  chain (max depth 8, immediate key must differ from parent); `AscendingBy`, `DescendingBy`
  factories; `WithTieBreaker`; deterministic `Compare(OrderingKey a, OrderingKey b)`.

## E2–EX — N/A
core-system has no commands, no queries, no engine handlers, no policy model,
no messaging constructs, no projections, no API endpoints, no workflows.

## E12 — Validation
- Sequence non-negativity, Next() monotonicity
- OrderingKey ordinal comparison determinism
- OrderingRule direction (ascending vs descending produces opposite ordering)
- TieBreaker chain: key-uniqueness invariant, max-depth invariant
- Tie-breaking: equal primary key falls through to TieBreaker correctly
- Serialization round-trip for all three types

# OUTPUT FORMAT

Per domain, provide:
1. Purpose statement
2. Value object(s) — production-ready code
3. Error class
4. Invariants
5. E12 test notes

# VALIDATION CRITERIA

- Sequence.Next() produces Value + 1, non-negative enforced
- OrderingKey.CompareTo uses Ordinal (culture-invariant)
- OrderingRule.Compare is deterministic and TieBreaker chain resolves correctly
- TieBreaker with same key as parent throws
- TieBreaker chain > 8 deep throws
- No infrastructure imports in any file
