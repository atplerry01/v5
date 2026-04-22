# TITLE
core-system / identifier — E1→EX Full Domain Batch

# CONTEXT

CLASSIFICATION: core-system
CONTEXT: identifier
DOMAIN GROUP: identifier
DOMAINS:
- global-identifier
- entity-reference
- correlation-id
- causation-id

Optional batch description:
Canonical identity and reference primitives — globally unique identifiers, typed entity
references, and causation/correlation chain links. All identifiers are SHA256-derived
(64 lowercase hex), immutable, and never generated inside the domain layer itself.

# OBJECTIVE

Implement and maintain the `identifier` domain group inside `core-system` as pure semantic
primitives. All identifiers must be immutable, globally unique where defined, deterministic,
and serialization-stable. Correlation/causation chains must be preserved across system
boundaries.

Reference: `/claude/project-topics/v3/core-system.md` — sections 1–8, 11–22.

# CONSTRAINTS

1. Domain layer ZERO external dependencies.
2. Identifiers are NEVER generated inside the domain — assigned by engine/runtime.
3. All types immutable: `readonly record struct` or `sealed record`.
4. GlobalIdentifier, CorrelationId, CausationId must be exactly 64 lowercase hex chars (SHA256).
5. EntityReference must follow `{classification}/{context}/{domain}` type format.
6. No aggregates with lifecycle, no commands, no events, no policy model.
7. core-system may not depend on any other classification.
8. All identifiers must support deterministic comparison and equality.

# EXECUTION STEPS

## E1 — Domain Primitives

### global-identifier
- `GlobalIdentifier` — 64 lowercase hex; `IComparable<GlobalIdentifier>` for stable sorting.

### entity-reference
- `EntityReference` — `(IdentifierValue: 64-hex, EntityType: classification/context/domain)`
  three-segment format; `IsSameEntity(EntityReference)` comparison.

### correlation-id
- `CorrelationId` — 64 lowercase hex; cross-boundary correlation link; never generated
  inside domain (assigned by engine from root trigger).

### causation-id
- `CausationId` — 64 lowercase hex; direct parent-child causal link; never generated
  inside domain (assigned by engine from inbound message ID).

## E2–EX — N/A
core-system has no commands, no queries, no engine handlers, no policy model,
no messaging constructs, no projections, no API endpoints, no workflows.

## E12 — Validation
- Empty / null value rejection
- Non-64-char rejection
- Non-hex-char rejection
- Uppercase rejection (must be lowercase)
- EntityReference three-segment format enforcement
- IComparable<GlobalIdentifier> deterministic ordering
- Equality: same value → equal; different value → not equal
- Serialization round-trip (ToString() produces stable output)
- CorrelationId / CausationId: independent types (same format, different semantics)
- Identifier never contains clock values or GUIDs from domain layer

# OUTPUT FORMAT

Per domain, provide:
1. Purpose statement
2. Value object(s) — production-ready code
3. Error class
4. Invariants
5. E12 test notes

# VALIDATION CRITERIA

- All four identifier types reject empty, non-64-hex inputs
- EntityReference rejects non-three-segment types
- GlobalIdentifier implements IComparable with Ordinal comparison
- EntityReference.IsSameEntity uses both identifier value and type
- No infrastructure imports in any file
- No domain-layer identifier generation
