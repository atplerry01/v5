# Deterministic ID Guard (HSID v2.1)

## Purpose

Lock the HSID v2.1 compact correlation-id format and its single source of
truth. This guard is the **second** deterministic identity guard alongside
[determinism.guard.md](determinism.guard.md), and the two are intentionally
non-overlapping:

| Guard | Seam | Output | Scope |
|-------|------|--------|-------|
| `determinism.guard.md` | `IIdGenerator.Generate(seed)` | `Guid` | Internal row ids, hash inputs, adapter envelopes |
| `deterministic-id.guard.md` (this) | `IDeterministicIdEngine.Generate(...)` | compact `string` | External-facing correlation IDs of the form `PPP-LLLL-TTT-TOPOLOGY-SEQ` |

A user-acknowledged drift capture for the parallel seam lives at
`claude/new-rules/20260407-200000-hsid-v2.1-parallel-seam.md`.

## Locked Format

```
PPP-LLLL-TTT-TOPOLOGY-SEQ
```

| Segment | Width | Charset | Meaning |
|---------|-------|---------|---------|
| PPP | 3 | `[A-Z]` | `IdPrefix` enum name |
| LLLL | 4 | `[A-Z]` | `LocationCode` |
| TTT | 3 | `[A-Z0-9]` | Deterministic time bucket (SHA256 of seed) |
| TOPOLOGY | 12 | `[A-Z0-9]` | Cluster (3) + SubCluster (3) + SPV (6) |
| SEQ | 3 | `[A-Z0-9]` | Bounded sequence (`X3`, 0..0xFFF) |

Canonical regex:

```
^[A-Z]{3}-[A-Z]{4}-[A-Z0-9]{3}-[A-Z0-9]{12}-[A-Z0-9]{3}$
```

## Rules

**G1 — SINGLE ENGINE.** All HSIDs MUST be produced by
`Whyce.Engines.T0U.Determinism.DeterministicIdEngine`. No other class may
construct an HSID literal or implement `IDeterministicIdEngine`.

**G2 — NO RANDOMNESS.** The engine, the bucket provider, and the sequence
resolver MUST NOT call `Guid.NewGuid`, `Random*`, `RandomNumberGenerator`,
`DateTime*.UtcNow`, `DateTimeOffset*.UtcNow`, `Environment.Tick*`, or
`Stopwatch.GetTimestamp`. The bucket is derived from the seed via SHA256.

**G3 — TOPOLOGY REQUIRED.** Every HSID MUST encode a `TopologyCode` of
exactly 12 characters: Cluster (3) + SubCluster (3) + SPV (6). A topology
value with any other width is a violation.

**G4 — SEQUENCE BOUNDED + LAST.** The sequence segment MUST be the LAST
segment, MUST be exactly 3 hex chars (`X3`), and MUST be produced by an
`ISequenceResolver` whose scope key includes BOTH the topology and the seed.
Unbounded counters or sequences keyed only on time are forbidden.

**G5 — DOMAIN PURITY.** No code under `src/domain/**` may inject or call
`IDeterministicIdEngine`. The domain layer is forbidden from naming its own
HSIDs.

**G6 — SINGLE STAMP POINT.** `IDeterministicIdEngine.Generate(...)` may be
called from EXACTLY two surfaces:
1. `src/runtime/control-plane/RuntimeControlPlane.cs` (the prelude that
   stamps `CommandContext.Hsid`).
2. `src/engines/T0U/determinism/**` (the engine itself, for self-tests).

A call from any other path is an architectural violation. The HSID is
stamped before the locked 8-middleware pipeline runs and is write-once on
`CommandContext.Hsid`.

**G7 — STRUCTURAL VALIDATION.** Every produced HSID MUST pass
`IDeterministicIdEngine.IsValid(id)` immediately after generation. The
prelude in `RuntimeControlPlane` performs this check; new call sites MUST
do the same.

**G8 — NO RUNTIME-ORDER MUTATION.** This guard MUST NOT be used as a
justification to add a 9th middleware to the locked pipeline in
`runtime-order.guard.md`. The HSID stamp lives in the control-plane prelude,
out of band of the 8-stage pipeline.

## Check Procedure

1. Ripgrep `src/` for `Guid.NewGuid`, `DateTime*.UtcNow`, `Random`,
   `Environment.Tick*`, `Stopwatch.GetTimestamp` inside
   `src/engines/T0U/determinism/**` and `src/shared/kernel/determinism/**`.
   Any hit → S0.
2. Ripgrep `src/` for `IDeterministicIdEngine` references. Any reference
   outside `src/runtime/control-plane/` and `src/engines/T0U/determinism/`
   → S1 (G6).
3. Ripgrep `src/domain/` for `IDeterministicIdEngine` or `HSID`. Any hit
   → S0 (G5).
4. Open `RuntimeControlPlaneBuilder.Build()` and confirm the locked list
   still contains exactly 8 middlewares (G8 cross-check vs
   `runtime-order.guard.md`).
5. Confirm the regex in `DeterministicIdEngine.Format` matches the canonical
   regex in this document character-for-character.

## Pass Criteria

- All HSIDs come through the single engine via the single prelude.
- Engine, bucket, and sequence are clock- and RNG-free.
- Topology is always 12 chars; sequence is always 3 hex chars.
- Domain layer is HSID-blind.
- Runtime middleware pipeline is still the locked 8-stage list.

## Fail Criteria

| Severity | Condition |
|----------|-----------|
| S0 | Random / clock primitive in `src/engines/T0U/determinism/**` or `src/shared/kernel/determinism/**` |
| S0 | Domain-layer code references `IDeterministicIdEngine` |
| S0 | Sequence segment width changed without updating both this guard and the engine regex |
| S1 | `IDeterministicIdEngine.Generate(...)` called outside the two permitted surfaces |
| S1 | A second `IDeterministicIdEngine` implementation is added |
| S2 | Topology derivation collapses to fewer than 12 chars |

## Enforcement Action

- S0 / S1 → block merge.
- S2 → must resolve in current PR.

---

## H2–H6 HARDENING ADDITIONS — 2026-04-07

**G12 — ENGINE REQUIRED.** `IDeterministicIdEngine`, `ISequenceResolver`,
and `ITopologyResolver` MUST all be configured. The
`RuntimeControlPlane` constructor throws on any null. There is no fallback,
no nullable injection, no optional DI. NO ENGINE → NO EXECUTION.

**G13 — SEQUENCE SOURCE.** The canonical sequence resolver is
`PersistedSequenceResolver` backed by `ISequenceStore`. The previous
`InMemorySequenceResolver` has been removed. Reintroducing an in-memory
resolver in production composition is a violation; the test
`InMemorySequenceStore` is permitted ONLY under
`tests/integration/setup/`.

**G14 — TOPOLOGY TRUST.** Topology MUST come from `ITopologyResolver` (via
`IStructureRegistry`) for any command that implements `IHsidCommand`.
Caller-supplied topology in a command body, request DTO, or HTTP header is
forbidden. The fallback path (non-`IHsidCommand`) derives topology
deterministically from `classification|context|domain` via SHA256 — this
fallback is permitted but flagged by audit A14.

**G15 — PRELUDE ENFORCEMENT.** HSID stamping MUST occur in the
`RuntimeControlPlane` prelude, before the locked 8-middleware pipeline runs.
No middleware may stamp or replace `CommandContext.Hsid`. The write-once
setter on `CommandContext.Hsid` enforces this at runtime.

**G16 — SEQUENCE STORE.** `ISequenceStore` MUST exist as a dedicated
persistence contract. `IEventStore` MUST NOT be used for HSID sequence
persistence. Cross-using the event store for sequence counters conflates
two replay surfaces.

**G17 — HSID COMMAND INTERFACE.** Commands MAY implement `IHsidCommand`. If
implemented, `RuntimeControlPlane` MUST resolve topology via
`ITopologyResolver`. A command that implements `IHsidCommand` but bypasses
the resolver is a G14 violation.

**G18 — SEQUENCE WIDTH.** Sequence segment MUST remain X3 (3 hex chars,
0..0xFFF). Any change to width MUST update this guard, the engine regex,
and the audit regex in the SAME commit. Locked 2026-04-07.

**G19 — INFRASTRUCTURE READINESS.** `ISequenceStore.HealthCheckAsync()`
MUST be invoked at host bootstrap by `HsidInfrastructureValidator`. The
runtime MUST NOT begin accepting traffic if the health check returns false
or throws. Silent degradation is forbidden.

**G20 — MIGRATION REQUIRED.** The `hsid_sequences` table MUST exist in
every environment that runs the host. Its canonical migration lives at
`infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql`. CI
MUST run `scripts/hsid-infra-check.sh` against the target database before
any deploy.

VIOLATION = BLOCKER.
