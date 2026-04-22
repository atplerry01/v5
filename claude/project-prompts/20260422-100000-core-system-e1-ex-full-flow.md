---
TITLE: core-system — Full E1→EX Flow Implementation
CLASSIFICATION: core-system
CONTEXT: all (identifier, ordering, temporal)
PHASES_IN_SCOPE: E1–E12 (E2/E4/E5/E6/E9/E11 N/A by design)
SOURCE: claude/project-topics/v3/core-system.md
---

# CONTEXT

core-system owns 3 domain groups (identifier, ordering, temporal) covering 11 leaf domains.
Per the topic file (topics 6, 10, 12), core-system explicitly has no commands, no events, no engines,
no policy model, and no workflow — it is pure value-object primitive definitions.

E1 (domain layer) and all prerequisite structural work were completed in the prior pass.
This prompt drives the remaining E1→EX deliverables.

# OBJECTIVE

Complete the full E1→EX flow for core-system. Given the topic constraints, the implementable
stages are E12 (comprehensive test coverage proving topics 19–22 completion criteria).

All other stages (E2, E4, E5, E6, E9, E11) are explicitly N/A for this classification.

# CONSTRAINTS

- Anti-Drift ($5): No architecture changes, no new patterns, no renaming
- Domain purity (D-PURITY-01): No STJ attributes in domain code
- Fix only: structural issues and template misalignment
- No redesign of domain logic

# EXECUTION STEPS

1. Implement CoreSystemIdentifierSerializationTests.cs (topics 13, 14, 16, 19)
   - JSON roundtrip for all 4 identifier types
   - Correlation propagation chain tests
   - Causation chain tests
   - Identifier collision resistance

2. Implement CoreSystemTemporalSerializationTests.cs (topics 5, 11, 13, 14, 20)
   - JSON roundtrip for all 4 temporal types
   - Timezone drift: various UTC offsets → UTC storage
   - Precision preservation to tick level

3. Implement CoreSystemOrderingSerializationTests.cs (topics 7, 13, 19, 20)
   - JSON roundtrip for Sequence, SequenceRange, OrderingKey
   - Ordering stability under repeated calls (antisymmetry, transitivity)
   - Monotonicity under replay

4. Implement CoreSystemRegressionTests.cs (topics 20, 22)
   - Replay consistency: same inputs → same value, 20× iterations
   - Cross-type safety: CorrelationId vs CausationId type distinction
   - Boundary precision: min-duration TimeRange, large Sequence
   - UTC normalization idempotency

# OUTPUT FORMAT

- 4 new test files added to tests/unit/core-system/
- No domain files modified
- All 11 domains covered

# VALIDATION CRITERIA

- JSON serialization produces deterministic, human-readable output for all types
- All types round-trip through serialize → parse → reconstruct → equal
- Replay consistency: 20 iterations of same construction produce equal values
- Timezone drift: any UTC offset → UTC stored → same instant comparison
- Ordering: antisymmetric, transitive, stable across repeated calls
- Topic 22 completion criteria fully met
