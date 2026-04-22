# ordering-rule

**Classification:** core-system
**Context:** ordering
**Domain Group:** ordering
**Domain:** ordering-rule

## Purpose

`OrderingRule` encodes a complete, deterministic ordering criterion: an `OrderingKey` to order by, an `OrderingDirection` (ascending or descending), and an optional `TieBreaker` chain for secondary (and tertiary, etc.) ordering when primary keys are equal.

This is the only place in the system that expresses *how* ordered data should be sorted. All ordering criteria are explicit, immutable, and culture-invariant.

## Invariants

- Direction is either `Ascending` or `Descending` — no ambiguity.
- TieBreaker key must differ from the rule's own key (no self-referential tie-breaking).
- TieBreaker chain depth must not exceed 8 levels.
- `Compare(a, b)` is deterministic: same inputs always produce the same result.
- Comparison uses `StringComparison.Ordinal` via `OrderingKey.CompareTo`.

## Domain Constraints (core-system)

- No aggregates, no lifecycle, no commands, no events.
- No policy logic, no access rules, no orchestration.
- Depends only on `ordering-key` within the same context.
- May not depend on any classification outside `core-system`.

## Usage

```csharp
var primary = OrderingRule.AscendingBy(new OrderingKey("created-at"));
var withTie = primary.WithTieBreaker(OrderingRule.DescendingBy(new OrderingKey("severity")));

int result = withTie.Compare(keyA, keyB);
```
