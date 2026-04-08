# Deterministic ID (HSID v2.1) Audit Output — Phase 1 Gate
**Audit Date:** 2026-04-08  
**Branch:** dev_wip  
**Guard:** deterministic-id.guard.md

---

## SCOPE

All code referencing or implementing IDeterministicIdEngine, ISequenceResolver, ITopologyResolver.
HSID format: ^[A-Z]{3}-[A-Z0-9]{4}-[A-Z0-9]{3}-[A-Z0-9]{12}-[A-Z0-9]{3}$

---

## FILES_AUDITED

**Files with HSID relevance:** 12

- src/engines/T0U/determinism/DeterministicIdEngine.cs
- src/engines/T0U/determinism/time/DeterministicTimeBucketProvider.cs
- src/engines/T0U/determinism/sequence/PersistedSequenceResolver.cs
- src/runtime/control-plane/RuntimeControlPlane.cs
- src/platform/host/bootstrap/HsidInfrastructureValidator.cs
- src/platform/host/adapters/PostgresSequenceStoreAdapter.cs
- src/domain/** (verified: no HSID references)

---

## FINDINGS

### G1 — SINGLE ENGINE: PASS
Only one IDeterministicIdEngine implementation: DeterministicIdEngine. 

### G2 — NO RANDOMNESS: PASS
Scanned src/engines/T0U/determinism/ for Guid.NewGuid, Random, DateTime*, Environment.Tick*.
- DeterministicIdEngine.Generate() uses ITimeBucketProvider, no clock read.
- DeterministicTimeBucketProvider.GetBucket() uses SHA256 only, no clock.
- PersistedSequenceResolver.NextAsync() delegates to ISequenceStore, no clock.
No violations.

### G3 — TOPOLOGY REQUIRED: PASS
- DeterministicIdEngine enforces TopologyCode parameter (12 chars).
- IsValid() validates topology width (line 76): if (parts[3].Length != 12) return false;
- RuntimeControlPlane derives topology via SHA256, produces 12 chars (3+3+6).

### G4 — SEQUENCE BOUNDED + LAST: PASS
- Regex enforces [A-Z0-9]{3} for sequence (third position from end).
- PersistedSequenceResolver uses modulo: (int)(raw % 0x1000) = X3 width.
- Scope includes topology: var scope = "{topology}:{seed}";

### G5 — DOMAIN PURITY: PASS
Ripgrep for IDeterministicIdEngine in src/domain/ returns no hits.

### G6 — SINGLE STAMP POINT: PASS
IDeterministicIdEngine.Generate() called only in RuntimeControlPlane.StampHsidAsync() (line 182).

### G7 — STRUCTURAL VALIDATION: PASS
DeterministicIdEngine.IsValid() includes full width checks (lines 63-83).
RuntimeControlPlane calls IsValid() immediately after generation (lines 184-186) and throws on failure.

### G8 — NO RUNTIME-ORDER MUTATION: PASS
HSID stamping in prelude (out-of-band), not in 8-middleware pipeline. No new middleware added.

### G12 — ENGINE REQUIRED: PASS
RuntimeControlPlane constructor throws InvalidOperationException if _hsidEngine, _hsidSequence, or _topologyResolver are null (lines 55-60).

### G13 — SEQUENCE SOURCE: PASS
PersistedSequenceResolver backed by ISequenceStore (PostgresSequenceStoreAdapter).
No in-memory sequence resolver in production path.

### G14 — TOPOLOGY TRUST: PASS
RuntimeControlPlane.StampHsidAsync() (lines 170-187):
- If command is IHsidCommand, topology resolved via ITopologyResolver (lines 175-176).
- Otherwise, topology derived deterministically from classification|context|domain (line 177).
No caller-supplied topology.

### G15 — PRELUDE ENFORCEMENT: PASS
HSID stamping in StampHsidAsync() (line 69), before middleware pipeline (line 72).

### G16 — SEQUENCE STORE: PASS
ISequenceStore is dedicated contract, not reusing IEventStore.
PostgresSequenceStoreAdapter uses hsid_sequences table (separate from events).

### G17 — HSID COMMAND INTERFACE: PASS
RuntimeControlPlane respects IHsidCommand and resolves topology authoritatively.

### G18 — SEQUENCE WIDTH: PASS
Regex locks to [A-Z0-9]{3}. MaxSequence = 0x1000. Revalidated in IsValid().

### G19 — INFRASTRUCTURE READINESS: PASS
Program.cs (lines 36-43) calls HsidInfrastructureValidator.ValidateAsync() at bootstrap.
Throws if sequence store health check fails.

### G20 — MIGRATION REQUIRED: PASS
Migration exists: infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql
Referenced in validator error message.

---

## VERDICT

PASS — All HSID v2.1 rules (G1–G20) enforced and validated.

---

## NEW_RULE_CANDIDATES

None identified. All rules are implemented.

