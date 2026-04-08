# determinism.audit.md

Post-execution audit consolidating determinism enforcement across all layers.
Pairs with `claude/guards/determinism.guard.md`.

## SCOPE
Determinism block list applies to:
- `src/domain/**`
- `src/engines/**`
- `src/runtime/**`
- `src/platform/host/adapters/**`
- All in-memory test doubles under `tests/**`

## CHECKS

### CHECK-DET-GUID-01 (S0)
grep `Guid.NewGuid` across the scope. Any hit = S0. Use `IIdGenerator.Generate(seed)`
with a deterministic seed (aggregate id, version, stream coordinate).

### CHECK-DET-CLOCK-01 (S0)
grep `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow`
across the scope. Any hit = S0 EXCEPT the single `SystemClock` (`IClock`)
implementation in `Program.cs`.

### CHECK-DET-RANDOM-01 (S0)
grep `new Random` / `Random.Shared` across the scope. Any hit = S0. Use a
seeded `IRandomSource` if randomness is required.

### CHECK-DET-SQL-01 (S2)
SQL `NOW()` / `CURRENT_TIMESTAMP` is permitted ONLY for audit columns the
application does not read back into deterministic logic. Any read of such a
column into application code = S2.

### CHECK-DET-CHAIN-01 (S1)
`ChainAnchor.BlockId` MUST be a deterministic hash of `(events, previousBlockId)`.
Any non-hash-derived `BlockId` = S1.

### CHECK-DET-EVENTSTORE-ID-01 (S0)
`PostgresEventStoreAdapter` row id MUST be `$"{aggregateId}:{version}"` via
`IIdGenerator`. Otherwise S0 — replays produce divergent physical row ids.

### CHECK-DET-PROJECTION-TS-01 (S1)
Kafka projection consumers MUST stamp envelope `Timestamp` from injected
`IClock.UtcNow`, NOT consume-moment wall clock. Otherwise projections couple
to scheduling and replays diverge.

### CHECK-DET-DI-WIRING-01 (S2)
`SystemClock` and `DeterministicIdGenerator` MUST be DI-registered as
singletons in `Program.cs`. All adapters above MUST inject them via
constructor — not new them up.

## SCORING

| Severity | Penalty |
|---|---|
| S0 violation | -40 per occurrence |
| S1 violation | -10 per occurrence |
| S2 violation | -5 per occurrence |
| S3 violation | -1 per occurrence |
| Floor | 0 |
| Pass threshold | >= 80 |

### CHECK: DET-POLICY-01
Verify policy decisions produce deterministic hashes.

## TRACEABILITY REFERENCE — 2026-04-07

MAP: see claude/traceability/guard-traceability.map.md
- Each CHECK in this audit maps to a Guard Rule ID, Enforcement Point, and Evidence as defined in the master traceability map.

## NEW CHECKS INTEGRATED — 2026-04-07 (HSID v2.1 parallel seam)

- **CHECK-DET-DUAL-SEAM-01** (S1): Assert both `IIdGenerator` and `IDeterministicIdEngine` exist with
  exactly one implementation each. Neither implementation may call `Guid.NewGuid`, `DateTime.UtcNow`,
  `Random`, or `Environment.Tick*`.
- **CHECK-DET-HSID-CALLSITE-01** (S1): Grep repo for `IDeterministicIdEngine` callers; any match
  outside `src/runtime/control-plane/` or `src/engines/T0U/determinism/` = violation.
- Source: `claude/new-rules/_archives/20260407-200000-hsid-v2.1-parallel-seam.md`.
