# Whycespace Domain

Pure DDD domain layer. ZERO external dependencies.

## Topology

`src/domain/{classification-system}/{context}/[{domain-group}/]{domain}/`

The `domain-group` segment is **optional** and chosen per-context. A context is either fully flat (3-level: `context/domain`) or fully grouped (4-level: `context/domain-group/domain`) — mixing within a single context is forbidden. See `claude/guards/domain.guard.md` DS-R3 / DS-R3a. `DomainRoute` remains a three-tuple `(classification, context, domain)` regardless of physical depth.

Currently 4-level: `content-system` (all contexts).
Currently 3-level: every other classification.

## Standard BC Structure

Each bounded context contains:
- `aggregate/` — Aggregate roots
- `entity/` — Domain entities
- `error/` — Domain error definitions
- `event/` — Domain events
- `service/` — Domain services
- `specification/` — Domain specifications
- `value-object/` — Value objects
