# R5.C.1 — Replay-Equivalence Certification

Classification: operational
Context: runtime-reliability
Domain: replay-determinism-certification

## TITLE
R5.C.1 — Formalize existing determinism proofs into a provenanced certification catalog; flag any uncovered invariant as an executable gap.

## CONTEXT
R1-R3 rest on a determinism claim: the runtime emits deterministic IDs / hashes / event streams / projection states, and replay produces byte-identical output. R5.C.1 promotes that claim from an architectural assertion to a guard-locked, executable invariant set — parallel to how R5.B cataloged the fault-to-alert mapping.

Recon confirms the runtime already carries substantial determinism coverage: `DeterministicReplayTest` (type A — command re-execution), `OutboundEffectAggregateReplayEquivalenceTests` (type B — aggregate stream reconstruction), `AggregateReplayHarness` (round-trip serialization), `Phase2DeterminismValidationTests` (id collision probe), workflow replay tests, and hash-determinism primitives (`ExecutionHash`, `ChainHasher`, `DeterministicIdGenerator`, `DeterministicRandomProvider`).

R5.C.1 does NOT build new determinism machinery — it catalogs what exists, pins the mapping via validators, and documents any gap.

## OBJECTIVE
Deliver a bounded replay-certification catalog:
- `infrastructure/observability/certification/replay-equivalence.yml` — machine-readable catalog of canonical determinism invariants with proof-test paths
- `tests/unit/certification/CanonicalReplayInvariants.cs` — C# mirror
- Validator tests that assert: manifest parses, every certified entry's proof file exists, mirror in sync with YAML, no certified invariant lacks a proof
- Guard rules locking the catalog as the single source of truth
- Explicit gap flagging for any invariant surfaced during recon that does not yet have executable proof

Not in scope:
- New runtime code / new determinism machinery
- Sustained-load chaos harness (R5.C.2)
- Soak SLO proving (R5.C.3)

## CONSTRAINTS
- Catalog MUST mirror R5.B's failure-mode pattern (YAML source of truth + C# mirror + validator tests).
- Every certified entry MUST point to an existing proof-test file; `unproven` entries MUST carry a `rationale:` block.
- No existing test changes — R5.C.1 is additive cataloging.
- No new runtime metrics / alerts / spans.

## EXECUTION STEPS (as delivered)
1. Prompt stored per $2.
2. Publish the replay-equivalence catalog under `infrastructure/observability/certification/` listing the canonical determinism invariants surfaced during recon, each pinned to its proof test.
3. Mirror the catalog as `CanonicalReplayInvariants.cs` so validators can consume it without a YAML parser.
4. Add validator tests under `tests/unit/certification/` — manifest parses, proof files exist, mirror is in sync.
5. Promote R5.C.1 guard rules into `runtime.guard.md` §R5.C.1 Replay-Equivalence Certification.
6. Record the closure + any gaps in `claude/audits/sweeps/`.

## OUTPUT FORMAT
Summary in conversation: invariants cataloged, gaps flagged, files, guards.

## VALIDATION CRITERIA
- Manifest + mirror + validator tests land.
- Every certified entry has an existing proof-test file on disk.
- R4.A / R4.B / R5.A / R5.B tests remain green.
- Any gap is documented as `unproven` with explicit rationale.

## DEFERRED (R5.C.2 / R5.C.3 scope)
- **Sustained-load chaos harness** — scripted load + scripted faults running under real Postgres/Kafka/OPA. R5.C.2.
- **Soak SLO proving** — multi-hour runs, memory stability, no leaks. R5.C.3.
- **Projection state byte-equivalence after full rebuild** — if recon flags this as a gap, catalog it as `unproven` for Phase 4 follow-up; not implementing new tests in R5.C.1.
