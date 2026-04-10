# Determinism Guard

## Purpose

Consolidate and extend the WBSM v3 determinism rules already scattered across
`behavioral.guard.md` (rule 16, GE-01), `domain.guard.md` (GE-01),
`engine.guard.md` (GE-01), and the runtime C# guard
`src/runtime/guards/DeterminismGuard.cs`.

This guard's distinct contribution: it **extends the determinism block list to
the platform adapter surface** (`src/platform/host/adapters/**`), which the
existing guards do not cover. Adapters are the persistence and event-fabric
boundary; non-determinism here breaks event-sourcing replay guarantees,
idempotency, deduplication, and chain anchoring even when domain/engine/runtime
code is perfectly deterministic.

## Scope

All code under:

- `src/domain/**` — already enforced by domain.guard.md GE-01 and
  behavioral.guard.md rule 16. Re-asserted here for completeness.
- `src/engines/**` — already enforced by engine.guard.md GE-01.
- `src/runtime/**` — already enforced and additionally checked at runtime by
  `src/runtime/guards/DeterminismGuard.cs`.
- **`src/platform/host/adapters/**`** — **NEW.** Added by this guard.
- `src/systems/**` — already implied by GE-01.

## Block List

Within all in-scope paths, the following are FORBIDDEN:

- `Guid.NewGuid()`
- `Guid.NewGuid().ToString(...)`
- `DateTime.Now`
- `DateTime.UtcNow`
- `DateTimeOffset.Now`
- `DateTimeOffset.UtcNow`
- `Random` instantiation, `Random.Shared`, `RandomNumberGenerator.GetBytes(...)`
  for non-cryptographic identity/sequence generation
- `Environment.TickCount`, `Environment.TickCount64`
- `Stopwatch.GetTimestamp()` used as an identity or event-stamp source

## Required Replacements

- For identity: `IIdGenerator.Generate(seed)` from
  `src/shared/kernel/domain/IIdGenerator.cs`. The seed MUST be derived
  deterministically from the operation's coordinates — for example
  `$"{aggregateId}:{version}"` for an event store row id, or
  `$"{commandId}:{handlerName}"` for a command-derived child id. Random or
  wall-clock seeds defeat the purpose.
- For time: `IClock.UtcNow` from `src/shared/kernel/domain/IClock.cs`.

Both seams are DI-registered as singletons in `src/platform/host/Program.cs`
(`SystemClock` → `IClock`, `DeterministicIdGenerator` → `IIdGenerator`).
There is no excuse for a constructor to be missing them.

## Exceptions

Exactly two surfaces are permitted to read the system clock or generate a
non-derived id, and they form the boundary between deterministic application
code and the underlying OS:

1. **The `IClock` implementation itself.** `SystemClock.UtcNow` in
   `src/platform/host/Program.cs` is the single permitted reader of
   `DateTimeOffset.UtcNow`. No other class.
2. **The `IIdGenerator` implementation itself.** Currently
   `DeterministicIdGenerator` in `src/platform/host/Program.cs`, which derives
   ids via `SHA256(seed)` and never reads the system clock or RNG. If a future
   implementation needs randomness, it must be confined to this single class.

SQL `NOW()` / `CURRENT_TIMESTAMP` inside SQL statements is permitted **only**
for audit-trail columns the application never reads back into deterministic
logic (e.g. `created_at` timestamps used solely for human-facing audit views).
A SQL-clock value that flows back into an aggregate, projection key, event
hash, or chain anchor is a violation.

## Check Procedure

1. Ripgrep the union of `src/domain`, `src/engines`, `src/runtime`,
   `src/systems`, and `src/platform/host/adapters` for each blocked pattern.
2. For every hit, classify:
   - Is the file the `IClock` or `IIdGenerator` implementation? → exception.
   - Is the hit inside a comment or string literal? → not a violation.
   - Otherwise → S0 violation.
3. For each adapter constructor, verify it accepts `IClock` and/or
   `IIdGenerator` whenever it stamps timestamps or generates ids.

## Pass Criteria

- Zero hits in the in-scope paths after exception filtering.
- Every adapter that stamps a timestamp injects `IClock`.
- Every adapter that generates a row id injects `IIdGenerator` and uses a
  deterministic seed.

## Fail Criteria

- Any blocked pattern in an in-scope path outside the two permitted exception
  files.
- An adapter that constructs an envelope/row with `DateTimeOffset.UtcNow` or
  `Guid.NewGuid()` inline.
- An `IIdGenerator.Generate(seed)` call where the seed is `Guid.NewGuid()`,
  `DateTimeOffset.UtcNow.Ticks`, or any other non-derived value.

## Severity

| Severity | Condition |
|----------|-----------|
| **S0 — CRITICAL** | Direct `Guid.NewGuid()` or `DateTime*.UtcNow` in domain, engine, runtime, systems, or platform adapter |
| **S0 — CRITICAL** | `IIdGenerator.Generate(seed)` called with a non-deterministic seed |
| **S1 — HIGH** | SQL clock value flowing back into a hash, key, or anchor |
| **S2 — MEDIUM** | New adapter added without `IClock` / `IIdGenerator` injection where it stamps or ids |

## Enforcement Action

- **S0**: Block merge. Phase 2 lock condition fails until resolved.
- **S1**: Block merge.
- **S2**: Warn in CI; must resolve in current PR.

## Relationship to other guards

This guard does not replace `behavioral.guard.md`, `domain.guard.md`,
`engine.guard.md`, or `runtime.guard.md` GE-01 sections. It supplements them
by widening the enforcement surface to platform adapters. Where this guard
and an existing guard overlap, the stricter rule wins.

---

## NEW RULES INTEGRATED — 2026-04-07

- **DET-ADAPTER-01**: Block list extended to src/platform/host/adapters/**. Forbidden: Guid.NewGuid(), DateTime.Now, DateTime.UtcNow, DateTimeOffset.Now, DateTimeOffset.UtcNow. Use IIdGenerator.Generate(seed) with deterministic seed derived from aggregate id/version/stream coordinate, and IClock.UtcNow.
- **DET-EXCEPTION-01**: The IClock implementation (SystemClock) is the ONLY permitted reader of DateTimeOffset.UtcNow. SQL NOW() / CURRENT_TIMESTAMP is permitted ONLY for audit columns the application does NOT read back into deterministic logic.
- **DET-SEED-01**: PostgresEventStoreAdapter row id MUST derive from "{aggregateId}:{version}" via IIdGenerator. Kafka projection envelopes MUST stamp Timestamp from IClock.UtcNow, not consume-moment wall clock.

## NEW RULES INTEGRATED — 2026-04-07 (HSID v2.1 parallel seam)

- **DET-DUAL-SEAM-01** (S1): The "single permitted ID seam" wording is reconciled. TWO deterministic
  identity seams are now canonical with non-overlapping responsibilities:
  (1) `IIdGenerator.Generate(seed)` — returns `Guid`, used for internal adapter/row/hash IDs, sole
  implementation `DeterministicIdGenerator` (SHA256 of seed → Guid).
  (2) `IDeterministicIdEngine.Generate(...)` — returns compact string
  `PPP-LLLL-TTT-TOPOLOGY-SEQ` for external-facing correlation IDs, sole implementation
  `Whyce.Engines.T0U.Determinism.DeterministicIdEngine`. Both must remain free of `Guid.NewGuid`,
  `DateTime*.UtcNow`, `Random`, `Environment.Tick*`.
- **DET-HSID-CALLSITE-01** (S1): `IDeterministicIdEngine.Generate(...)` MUST NOT be called outside
  `src/runtime/control-plane/` and `src/engines/T0U/determinism/`.
- Source: `claude/new-rules/_archives/20260407-200000-hsid-v2.1-parallel-seam.md`.

## NEW RULES INTEGRATED — 2026-04-10 (promoted from new-rules backlog)

- **DET-SEED-DERIVATION-01** (S1): When invoking `IIdGenerator.Generate(seed)` (or any seam producing a deterministic identifier from a seed string), the seed MUST be composed exclusively of stable command coordinates (aggregate id, command type name, aggregate version, correlation/causation id, deterministic discriminators). FORBIDDEN seed components: `IClock.UtcNow`/`DateTime.*`/`Stopwatch.*`/`Ticks`, `Guid.NewGuid()`/`Random.*`/`RandomNumberGenerator.*`, process/thread/machine identifiers, env vars, or hashes thereof. Static check: search `IIdGenerator.Generate(` and flag any seed-string interpolation containing `Clock|Now|Ticks|Guid|Random`. Architecture-test enforcement under `tests/unit/architecture/WbsmArchitectureTests`. Rationale: non-deterministic seeds defeat the entire deterministic-id mechanism and silently break replay, projection idempotency, and chain integrity. Source: `_archives/20260408-103326-determinism.md`.
- **DET-IDCHECK-COVERAGE-01** (S2): `scripts/deterministic-id-check.sh` (or a sibling script) MUST scan `tests/**` and `scripts/validation/**` in addition to `src/**`. Test paths and validation harnesses are not exempt from determinism rules. Source: `_archives/20260408-142631-validation.md` Finding 4.
