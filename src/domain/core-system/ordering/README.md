# core-system / ordering

**Classification:** core-system  
**Context:** ordering

## Purpose
Ordering primitives: immutable structural types for monotonic position and stable key-based ordering within any ordered set. No generation, no mutation — only structural expression.

## Sub-domains
| Domain | Responsibility |
|---|---|
| `sequence` | Monotonic integer position within an ordered set |
| `ordering-key` | Stable, comparable key for deterministic ordering of entities |
| `ordering-rule` | Deterministic ordering criterion: key + direction + optional tie-breaker chain |

## Does Not Own
- Sequence generation or incrementing (→ engine layer)
- Domain-specific key values (→ consuming domain)

## Invariants
- All values are immutable once constructed
- No behavior beyond structural validation and comparison

## Dependencies
- None
