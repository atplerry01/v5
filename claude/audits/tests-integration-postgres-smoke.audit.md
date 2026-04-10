# tests-integration-postgres-smoke.audit.md

## PURPOSE

Run the Postgres-gated subset of the integration suite at least once per gate, against either an env-var-supplied database or an ephemeral container if no env var is present. Catches production runtime bugs in the Postgres adapter surface that compile cleanly but fail at first connection.

## SOURCE

Promoted from `claude/new-rules/_archives/20260409-120500-infrastructure-tests-integration-baseline-drift.md`. The originating session uncovered an S0 production runtime bug in `src/platform/host/adapters/PostgresPoolMetrics.cs:170` (`OpenInstrumentedAsync`): the Npgsql library upgrade removed support for the `NpgsqlConnection.Disposed` event, which now throws `NotSupportedException` at subscription time. Blast radius covered every production code path through `OpenInstrumentedAsync` (`KafkaOutboxPublisher.PublishBatchAsync`, `PostgresOutboxAdapter.EnqueueAsync`, `PostgresEventStoreAdapter.LoadEventsAsync`/`AppendEventsAsync`, `PostgresIdempotencyStoreAdapter`, `OutboxDepthSampler`). The bug went undetected because the Postgres-gated integration tests were broken at compile time (TB-1 baseline drift) and therefore never ran. This audit runs them every gate.

## CHECK

### CHECK-TIPS-01 (S0) — Postgres-gated tests execute green

Run the Postgres-gated subset of `tests/integration/`:

```
dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
  --filter "Category=Postgres|FullyQualifiedName~Postgres" \
  -nologo
```

If the env var `WHYCE_INTEGRATION_POSTGRES_CS` is unset, the audit MUST start an ephemeral Postgres container (matching the `whyce_eventstore` schema produced by `bootstrap.sh`), set the env var, run the tests, and tear it down. FAIL S0 on any non-zero exit.

### CHECK-TIPS-02 (S0) — `OpenInstrumentedAsync` is exercised

The Postgres-gated subset MUST include at least one test that opens a connection through `PostgresPoolMetrics.OpenInstrumentedAsync` and asserts the in-flight pool counter increments and decrements correctly across `Dispose`. This is the regression test for the `Disposed` → `StateChange(Closed)` fix and prevents a future Npgsql upgrade from re-introducing the same class of failure.

## REMEDIATION

On failure, the underlying production adapter is broken — fix the adapter, not the test. Do not skip Postgres-gated tests to make the suite green.

## SEVERITY

S0 — production data path. A green build with red Postgres tests means the application cannot persist anything in production.
