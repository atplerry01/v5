# tests-integration-build.audit.md

## PURPOSE

Convert `tests.guard.md §T-BUILD-01` from a documented rule into an executing post-execution gate. The integration test project MUST compile on every CI run; a red integration project halts merge.

## SOURCE

Promoted from `claude/new-rules/_archives/20260409-120500-infrastructure-tests-integration-baseline-drift.md`. The originating drift went undetected because no audit ran the integration build as a precondition. When MI-2 attempted to add a new test it discovered 21 pre-existing compile errors at HEAD on `dev_wip`, all from production interface evolution that was never propagated into `tests/integration/setup/` doubles. The same baseline drift had also masked an S0 production runtime bug in `PostgresPoolMetrics.OpenInstrumentedAsync` (Npgsql `Disposed` event removal) — the bug only surfaced after the build was repaired and the Postgres-gated tests finally ran.

## CHECK

### CHECK-TIB-01 (S0) — Integration project compiles

Run:

```
dotnet build tests/integration/Whycespace.Tests.Integration.csproj -nologo
```

FAIL with severity S0 if the exit code is non-zero. Capture the full compiler output in the audit report.

### CHECK-TIB-02 (S1) — Test doubles co-evolve with production interfaces

For each commit being audited, if the diff touches a constructor signature or interface member declaration in `src/**`, assert that the corresponding test doubles in `tests/integration/setup/` (and any directly-coupled test files) were updated in the SAME commit. Orphaned test doubles whose target interface has drifted = S1 architectural violation.

Common drift surfaces (tracked from the originating sweep):
- `IEventStore` overload changes → `StubEventStore`, `MutableEventStore`
- `IIdempotencyStore` member additions → `InMemoryIdempotencyStore`
- `IMiddleware.ExecuteAsync` signature → `RecordingMiddleware`
- `ChainAnchorService` constructor → `TestHost`, any direct test instantiation
- `RuntimeCommandDispatcher` constructor → `TestHost`
- `PostgresEventStoreAdapter` / `KafkaOutboxPublisher` constructor (`EventStoreDataSource`, `IWorkerLivenessRegistry`, `IClock`) → respective integration tests

## REMEDIATION

On CHECK-TIB-01 fail, fix the test doubles in the SAME pass that introduced the production change. Do NOT sideline broken test files or guard them out — repair them. Mechanical alignment is the canonical response (see resolution log in the source archive for the TB-1 pattern).

## SEVERITY

S0 for compile failure (CHECK-TIB-01). S1 for orphaned doubles (CHECK-TIB-02).
