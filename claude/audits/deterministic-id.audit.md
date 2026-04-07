# Deterministic ID Audit (HSID v2.1)

Mirror of [deterministic-id.guard.md](../guards/deterministic-id.guard.md).
Run after any change touching `src/engines/T0U/determinism/**`,
`src/shared/kernel/determinism/**`, or
`src/runtime/control-plane/RuntimeControlPlane.cs`.

## A1 — Block-list ripgrep

Inside `src/engines/T0U/determinism/**` and `src/shared/kernel/determinism/**`:

- `Guid.NewGuid`
- `DateTime.Now`, `DateTime.UtcNow`
- `DateTimeOffset.Now`, `DateTimeOffset.UtcNow`
- `Random`, `Random.Shared`, `RandomNumberGenerator`
- `Environment.TickCount`, `Environment.TickCount64`
- `Stopwatch.GetTimestamp`

Any hit (outside string literals / comments) FAILS the audit.

## A2 — Format regex

Confirm `DeterministicIdEngine.Format` is exactly:

```
^[A-Z]{3}-[A-Z]{4}-[A-Z0-9]{3}-[A-Z0-9]{12}-[A-Z0-9]{3}$
```

Any width drift FAILS — and ALSO requires the matching guard regex update.

## A3 — Topology trace

Verify `TopologyCode.ToString()` concatenates Cluster (3) + SubCluster (3) +
SPV (6) and that `DeterministicIdEngine.Generate` rejects values whose total
length is not 12.

## A4 — Bucket derivation

Verify `DeterministicTimeBucketProvider.GetBucket(seed)` returns
`Convert.ToHexString(SHA256(seed))[..3]` and reads no clock.

## A5 — Single engine + single stamp point

Ripgrep `src/` for the symbol `IDeterministicIdEngine`. Permitted hits:

- `src/shared/kernel/determinism/IDeterministicIdEngine.cs` (the contract)
- `src/engines/T0U/determinism/DeterministicIdEngine.cs` (the impl)
- `src/runtime/control-plane/RuntimeControlPlane.cs` (the stamp prelude)
- `src/platform/host/composition/runtime/RuntimeComposition.cs` (DI wiring)

Any other hit FAILS (G6 violation).

## A6 — Domain purity

Ripgrep `src/domain/` for `IDeterministicIdEngine`, `Whyce.Shared.Kernel.Determinism`,
`HSID`, `TopologyCode`. ANY hit FAILS (G5 violation, S0).

## A7 — Pipeline shape preserved

Open `src/runtime/control-plane/RuntimeControlPlaneBuilder.cs`. Confirm
`Build()` still returns exactly 8 middlewares in the order locked by
`runtime-order.guard.md`. If the prelude addition tempted anyone to add a
9th middleware, this audit FAILS.

## A8 — Write-once Hsid

Open `src/shared/contracts/runtime/CommandContext.cs`. Confirm `Hsid` is
backed by a write-once setter that throws on second assignment, mirroring
the existing `IdentityId` / `PolicyDecision*` pattern.

## H2–H6 HARDENING CHECKS — 2026-04-07

### A9 — Engine presence

Open `src/runtime/control-plane/RuntimeControlPlane.cs`. The constructor MUST
throw on null `IDeterministicIdEngine`, null `ISequenceResolver`, and null
`ITopologyResolver`. FAIL if any of these deps are nullable, optional, or
defaulted.

### A10 — Sequence implementation

Ripgrep `src/` for `InMemorySequenceResolver`. Any hit FAILS — the type was
removed in H3. Ripgrep `src/platform/host/composition/` for
`InMemorySequenceStore`. Any hit FAILS — the in-memory store is permitted
only under `tests/integration/setup/`.

### A11 — Topology source

Ripgrep `src/` for the symbol `TopologyCode(` constructor invocations.
Permitted call sites:

- `src/runtime/topology/TopologyResolver.cs` (authoritative path)
- `src/runtime/control-plane/RuntimeControlPlane.cs` (`DeriveTopology` fallback)
- `src/engines/T0U/determinism/**` (engine self-tests)

Any other invocation FAILS — topology must not be constructed from request
data.

### A12 — Validation strength

Open `src/engines/T0U/determinism/DeterministicIdEngine.cs`. `IsValid` MUST:

- regex-match the canonical pattern,
- re-check segment counts and per-segment widths (PPP=3, LLLL=4, TTT=3,
  TOPOLOGY=12, SEQ=3),
- validate that the prefix parses as a known `IdPrefix` enum value.

FAIL if `IsValid` only checks segment count or only runs the regex.

### A13 — Sequence backend

Ripgrep `src/` for `IEventStore` references inside
`src/engines/T0U/determinism/**`. Any hit FAILS (G16). Ripgrep `src/` for
`InMemorySequenceResolver` and `InMemorySequenceStore` outside
`tests/integration/setup/`. Any hit FAILS.

### A14 — Topology fallback

Open `RuntimeControlPlane.StampHsidAsync`. Verify the non-`IHsidCommand`
branch is still deterministic (SHA256 over `classification|context|domain`)
and clock-free / RNG-free. FAIL if the fallback path reads any
non-deterministic input.

### A15 — Sequence width

The `SEQ` segment MUST be exactly 3 chars. Any audit run that observes a
generated HSID with a SEQ segment of length ≠ 3 FAILS. Cross-check the
engine's per-segment validation in A12.

### A16 — Infra validation

Open `src/platform/host/Program.cs`. Confirm that
`HsidInfrastructureValidator.ValidateAsync()` is awaited inside a DI scope
BEFORE `app.Run()` (and before any HTTP middleware that handles user
traffic). FAIL if:

- the validator is not resolved at startup,
- the call is wrapped in a try/catch that swallows the exception,
- or the host can reach `app.Run()` while the sequence store is unhealthy.

### A17 — Migration check

Confirm `infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql`
exists and creates the `hsid_sequences(scope text PK, next_value bigint)`
table. FAIL if the migration is missing, renamed without updating
`HsidInfrastructureValidator`, or weakened (e.g. column types changed).

Confirm `scripts/hsid-infra-check.sh` exists and is wired into CI.

## Outcome

Any FAIL above is reported under `claude/new-rules/` for promotion review,
even if the originating change is otherwise correct.
