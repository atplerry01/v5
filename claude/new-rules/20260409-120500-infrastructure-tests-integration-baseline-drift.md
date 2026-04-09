---
classification: infrastructure
context: tests-integration
discovered_during: phase1.5-S5.2.5 MI-2 verification
severity: S0 (T-BUILD-01 violation)
type: audits
---

# CLASSIFICATION
infrastructure / tests-integration / baseline-drift

# SOURCE
Discovered during phase1.5-S5.2.5 MI-2 (Outbox Multi-Instance Safety
Verification). The MI-2 task added a new test file
`tests/integration/platform/host/adapters/OutboxMultiInstanceSafetyTest.cs`
and attempted to build the integration project per T-BUILD-01. The build
failed with errors entirely in PRE-EXISTING files at HEAD (none in the
new MI-2 file).

# DESCRIPTION

The integration test project `Whycespace.Tests.Integration.csproj` does
NOT compile at HEAD on `dev_wip`. This is a standing T-BUILD-01 violation
(`tests/integration/ MUST compile on every CI run`). The drift comes from
production interface evolution that was never propagated into the test
doubles, fixtures, and adapters under `tests/integration/`.

Errors observed (full set, after a clean build of the integration csproj):

## Round 1 — initial 7 errors

1. `tests/integration/orchestration-system/workflow/execution/WorkflowPayloadTypeTests.cs`:
   `StubEventStore` is missing the new `LoadEventsAsync(Guid, CancellationToken)`
   and `AppendEventsAsync(Guid, IReadOnlyList<object>, int, CancellationToken)`
   overloads on `IEventStore`.

2. `tests/integration/orchestration-system/workflow/execution/WorkflowReplayResumeTests.cs`:
   `MutableEventStore` — same `IEventStore` overload drift.

3. `tests/integration/setup/InMemoryIdempotencyStore.cs`:
   missing `IIdempotencyStore.TryClaimAsync(string, CancellationToken)` and
   `ReleaseAsync(string, CancellationToken)`.

4. `tests/integration/setup/RecordingMiddleware.cs`:
   `IMiddleware.ExecuteAsync` signature drift —
   the production interface now takes
   `(CommandContext, object, Func<CancellationToken, Task<CommandResult>>, CancellationToken)`.

## Round 2 — surfaced after sidelining round-1 files (additional 14 errors)

5. `tests/integration/setup/TestHost.cs`:
   - References stale `InMemoryIdempotencyStore` and `RecordingMiddleware` types.
   - `ChainAnchorService` constructor now requires `ChainAnchorOptions` —
     not supplied.
   - `RuntimeCommandDispatcher` constructor now requires
     `WorkflowAdmissionGate` — not supplied.

6. `tests/integration/orchestration-system/workflow/execution/WorkflowResumedEventFabricRoundTripTest.cs`:
   `ChainAnchorService` missing `options` argument.

7. `tests/integration/eventstore/PostgresEventStoreConcurrencyTest.cs`:
   `PostgresEventStoreAdapter` constructor signature changed from
   `(string connectionString, ...)` to `(EventStoreDataSource, ...)`.
   Four call sites in this file still pass a `string`.

8. `tests/integration/platform/host/adapters/KafkaOutboxPublisherConfigTests.cs`:
   `KafkaOutboxPublisher` constructor no longer accepts a
   `connectionString:` named parameter (it now requires `EventStoreDataSource`,
   `IWorkerLivenessRegistry`, and `IClock`). Five call sites broken.

# IMPACT

- T-BUILD-01 is silently failing on `dev_wip`. Any verification task that
  requires building the integration project (such as MI-2) cannot proceed
  to the "all tests passing" acceptance criterion without first repairing
  this baseline.
- The drift was not caught earlier because no audit currently runs
  `dotnet build tests/integration/Whycespace.Tests.Integration.csproj`
  as a precondition — T-BUILD-01 is documented in `tests.guard.md` but
  has no executing audit definition.
- Worse: the canonical `PostgresEventStoreConcurrencyTest` template that
  new Postgres-gated tests should mirror is itself broken at HEAD,
  silently teaching the wrong constructor pattern.

# PROPOSED RULE (PROMOTION CANDIDATE)

## NEW AUDIT: `tests-integration-build.audit.md`

Add a post-execution audit that runs:

```
dotnet build tests/integration/Whycespace.Tests.Integration.csproj -nologo
```

and FAILS with severity S0 if the exit code is non-zero. This converts
T-BUILD-01 from a guard-document rule into an executing gate that the
$1b sweep can enforce.

## NEW GUARD ADDENDUM: `tests.guard.md` §T-BUILD-01

Strengthen the existing T-BUILD-01 wording to require that production
interface changes which alter constructor signatures or add interface
members MUST update the corresponding test doubles in
`tests/integration/setup/` in the SAME commit. Treat orphaned test
doubles as an S1 architectural violation.

## REMEDIATION SCOPE (separate task)

Repairing the baseline is OUT OF SCOPE for MI-2 (per $5 anti-drift) but
should be tracked as its own phase task. Suggested task name:
`phase1.5-S5.2.6 / TB-1 (TESTS-INTEGRATION-BASELINE-REPAIR)`. Required
work:

  1. Update `StubEventStore` and `MutableEventStore` to current `IEventStore`.
  2. Update `InMemoryIdempotencyStore` to current `IIdempotencyStore`.
  3. Update `RecordingMiddleware` to current `IMiddleware`.
  4. Update `TestHost` constructor wiring for `ChainAnchorService` and
     `RuntimeCommandDispatcher`.
  5. Update `PostgresEventStoreConcurrencyTest` and
     `KafkaOutboxPublisherConfigTests` to the current
     `EventStoreDataSource` / `IWorkerLivenessRegistry` / `IClock`
     constructor surface of their respective adapters.
  6. After repair, this captured rule can be promoted to a permanent
     audit and archived under `claude/new-rules/_archives/`.

# MI-2 CONSEQUENCE

The MI-2 verification file `OutboxMultiInstanceSafetyTest.cs` is
structurally clean against current production interfaces — it appears in
none of the 21 errors above. It cannot, however, be **executed** as part
of a green integration build until TB-1 is completed. Until then, the
proof of MI-2 is:

  - Static: the WHY documentation block on `KafkaOutboxPublisher.PublishBatchAsync`
  - Pending dynamic: the three test methods in `OutboxMultiInstanceSafetyTest.cs`,
    which will run as soon as the integration baseline is restored.

# RESOLUTION (in-session)

TB-1 was authorized and resolved in the same session as MI-2. All 8 broken
files were mechanically aligned with current production interfaces:

- `tests/integration/setup/InMemoryIdempotencyStore.cs` — added
  `TryClaimAsync` / `ReleaseAsync` (KC-2 contract).
- `tests/integration/setup/RecordingMiddleware.cs` — updated to the
  post-TC-1 `IMiddleware` signature with `Func<CT, Task<...>>` `next`.
- `tests/integration/setup/TestHost.cs` — supplied `ChainAnchorOptions`
  (KW-1) to `ChainAnchorService` and `WorkflowAdmissionGate` (KC-6) to
  `RuntimeCommandDispatcher`.
- `tests/integration/orchestration-system/workflow/execution/WorkflowPayloadTypeTests.cs`
  and `WorkflowReplayResumeTests.cs` — updated `StubEventStore` /
  `MutableEventStore` for the post-TC-5 `IEventStore` CT contract.
- `tests/integration/orchestration-system/workflow/execution/WorkflowResumedEventFabricRoundTripTest.cs`
  — supplied `ChainAnchorOptions` to `ChainAnchorService`.
- `tests/integration/eventstore/PostgresEventStoreConcurrencyTest.cs` —
  wrapped raw connection strings in `EventStoreDataSource`
  (`NpgsqlDataSource.Create` + `new EventStoreDataSource(ds)`) at all
  four call sites (PC-4 contract).
- `tests/integration/platform/host/adapters/KafkaOutboxPublisherConfigTests.cs`
  — rewritten end-to-end for the post-PC-4 / HC-5 publisher constructor
  (`EventStoreDataSource` + `IWorkerLivenessRegistry` + `IClock`).

# LATENT S0 PRODUCTION BUG SURFACED BY TB-1

After TB-1 restored compilation, the first execution of the
`PostgresEventStoreConcurrencyTest` suite revealed a separate, far more
serious finding:

```
System.NotSupportedException : The Disposed event isn't supported by
Npgsql. Use DbConnection.StateChange instead.
   at Whyce.Platform.Host.Adapters.PostgresPoolMetrics.OpenInstrumentedAsync
   (NpgsqlDataSource dataSource, String poolName, CancellationToken ct)
   in src/platform/host/adapters/PostgresPoolMetrics.cs:line 170
```

`PostgresPoolMetrics.OpenInstrumentedAsync` (HC-6) subscribed to
`NpgsqlConnection.Disposed` to decrement the in-flight pool counter when
the caller returned a connection to the pool. The Npgsql library was
upgraded at some point and removed support for the `Disposed` event,
which now throws `NotSupportedException` at subscription time.

**Blast radius:** every production code path through
`OpenInstrumentedAsync` was broken at runtime against the upgraded Npgsql,
including:

  - `KafkaOutboxPublisher.PublishBatchAsync`
  - `PostgresOutboxAdapter.EnqueueAsync`
  - `PostgresEventStoreAdapter.LoadEventsAsync` / `AppendEventsAsync`
  - `PostgresIdempotencyStoreAdapter` (all four methods)
  - `OutboxDepthSampler`

The bug went undetected because the integration tests that exercise these
seams against a real Postgres were broken at compile time (the TB-1
baseline drift documented above) and therefore never ran. TB-1 surfaced
both at once: fixing the test compilation immediately exposed the
production runtime bug on first execution.

**Fix applied (mechanical):** replaced the unsupported `Disposed` event
subscription with the canonical `StateChange` event filtered to the
`Closed` transition, which is the moment Npgsql signals connection return
during `Dispose`. The HC-6 in-flight counter behavior is preserved
exactly. Single-file change at `src/platform/host/adapters/PostgresPoolMetrics.cs`,
lines 164-189.

**Verification:** after the fix, the full integration suite (62 tests)
runs green, including all four `PostgresEventStoreConcurrencyTest`
methods that previously failed with the `NotSupportedException`.

# PROMOTION CANDIDATES

Based on this session, the following audit / guard work should be
promoted into the canonical set:

1. **`tests-integration-build.audit.md`** — runs
   `dotnet build tests/integration/Whycespace.Tests.Integration.csproj`
   and fails S0 on non-zero exit. Would have caught the TB-1 baseline
   drift the moment it was introduced.

2. **`tests-integration-postgres-smoke.audit.md`** — runs the Postgres-
   gated subset of the integration suite at least once per gate (against
   ephemeral container if no env var). Would have caught the
   `Disposed`-event production bug on the first commit that bumped Npgsql.

3. **`tests.guard.md` §T-BUILD-01 strengthening** — declare it an S1
   architectural violation for a commit that changes a production
   interface signature (constructors or interface members) without also
   updating the corresponding test doubles in `tests/integration/setup/`.
   Same commit, same gate.

# SEVERITY
S0 — system-breaking for T-BUILD-01.
S0 — production runtime bug in `PostgresPoolMetrics.OpenInstrumentedAsync`
(latent against current Npgsql, fixed in this session).
