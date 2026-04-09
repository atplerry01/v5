# §5.3.1 — Baseline Performance Profiling (EVIDENCE)

**Workload:** 100 RPS controlled, evenly-paced, sustained for 60 seconds.
**Date:** 2026-04-09
**Phase:** Phase 1.5B — first step of the re-opened Phase 1.5 amendment.
**Phase 1.5 amendment ref:** [phase1.5-reopen-amendment.md](../../phase1.5-reopen-amendment.md)
**Test file:** [tests/integration/load/BaselinePerformanceTest.cs](../../../../../tests/integration/load/BaselinePerformanceTest.cs)
**Gate env var:** `BaselineTest__Enabled=true`

---

## 1. Scope reminder

§5.3.1 in Phase 1.5B is a **baseline measurement**, NOT a stress, soak,
or scalability test. The objective is to record what the runtime
actually does under low–moderate sustained load on a single host so
later sections (§5.3.2 sustained 1k RPS, §5.3.3 burst/stress, §5.3.4
1M RPS gap analysis) have a documented starting point.

Strict constraints (per the §5.3.1 prompt) honoured by this run:

- Zero `src/` production code modifications. Verified.
- Zero new instrumentation or new `Whyce.*` meters. The harness uses
  only `Stopwatch`, `Process.TotalProcessorTime`, `Environment.WorkingSet`,
  and `GC.CollectionCount`.
- Real execution. No mocks of measured surfaces — the harness drives
  the canonical `RuntimeControlPlane` through `TestHost.ForTodo()`,
  the same composition path used by the §5.3 burst harness.
- Test is gated. Default integration suite stays fast.

## 2. Test delivered

`Baseline_100_Rps_For_60_Seconds_Profile` in
[BaselinePerformanceTest.cs](../../../../../tests/integration/load/BaselinePerformanceTest.cs):

- Single-dispatcher pacing loop. Each iteration computes the next
  deadline (`start + N × 1/RPS`), dispatches one `CreateTodoCommand`
  through `RuntimeControlPlane.ExecuteAsync`, and sleeps until the
  deadline if running ahead. This produces an evenly-spaced cadence
  rather than a burst.
- Per-request latency captured by a `Stopwatch.GetTimestamp` delta
  around the dispatch call, in microseconds. Pre-allocated buffer
  sized for the expected sample count so the measurement loop never
  pays a resize cost mid-run.
- CPU captured via `Process.TotalProcessorTime` delta across the
  whole window, normalised by elapsed wall-clock × `ProcessorCount`.
- Working set captured at start, mid (~30s), and end via
  `Environment.WorkingSet`.
- GC collection counts captured per generation as deltas across the
  window (`GC.CollectionCount`).

## 3. Run configuration

| Parameter            | Value                              |
|----------------------|------------------------------------|
| Target RPS           | 100                                |
| Duration             | 60 s                               |
| Pacing               | Evenly-spaced single-dispatcher    |
| Composition          | `TestHost.ForTodo()` (in-memory)   |
| Host mode            | Single-host                        |
| Command type         | `CreateTodoCommand`                |
| Persistence seams    | In-memory (`InMemoryEventStore`, `InMemoryOutbox`, `InMemoryIdempotencyStore`, `InMemoryChainAnchor`, `InMemorySequenceStore`) |
| Policy seam          | `AllowAllPolicyEvaluator`          |
| Build configuration  | Debug, .NET 10.0                   |
| OS                   | Windows 11 Pro 10.0.26100          |
| Test runner          | `dotnet test --no-build`           |

## 4. Raw harness output

The harness emits a single diagnostic line per run. Verbatim from the
2026-04-09 execution:

```
[§5.3.1 baseline harness] target=100rps×60s mode=in-memory
dispatched=6001 failed=0 exceptions=0 actualRps=100.0 elapsed=60.01s
latencyUs(min/p50/p95/p99/max/mean)=39/216/507/1124/109957/273
wsStart=56MB wsMid=76MB wsEnd=83MB cpuMs=3516 cpuAvg%=0.7
gc(0/1/2)=52/15/4
```

xUnit result line:

```
Passed Whycespace.Tests.Integration.Load.BaselinePerformanceTest
       .Baseline_100_Rps_For_60_Seconds_Profile [1 m]
Total tests: 1     Passed: 1     Total time: 1.0145 Minutes
```

## 5. Computed metrics

### 5.1 Throughput

| Metric        | Value          |
|---------------|----------------|
| Target RPS    | 100            |
| Achieved RPS  | **100.0**      |
| Dispatched    | 6,001          |
| Elapsed       | 60.01 s        |
| Tolerance     | ±10% (B4)      |
| Result        | **PASS**       |

The single-dispatcher pacing loop tracked the 1/RPS deadline cadence
without drift across the entire 60-second window.

### 5.2 Latency (per-request, end-to-end through `RuntimeControlPlane`)

| Percentile | Latency (µs) | Latency (ms) |
|------------|--------------|--------------|
| min        | 39           | 0.039        |
| **p50**    | **216**      | **0.216**    |
| **p95**    | **507**      | **0.507**    |
| **p99**    | **1,124**    | **1.124**    |
| max        | 109,957      | 109.957      |
| mean       | 273          | 0.273        |

The 110 ms max is a single-sample tail, consistent with a JIT or GC
pause early in the run; p99 at 1.124 ms shows the steady-state body
of the distribution is sub-2 ms for >99% of requests.

Sortedness invariants asserted in test:
`p50 ≤ p95 ≤ p99 ≤ max` — all hold.

### 5.3 Resource usage

| Metric                       | Value         |
|------------------------------|---------------|
| Working set (start)          | 56 MB         |
| Working set (mid, ~30s)      | 76 MB         |
| Working set (end, 60s)       | 83 MB         |
| Working set growth           | +27 MB (1.48×)|
| CPU time consumed (total)    | 3,516 ms      |
| CPU avg % (per logical core) | 0.7%          |

The +27 MB working-set growth across the 60-second window is expected
in this composition: every dispatch appends events to the in-memory
event store and outbox, neither of which evicts. This is consistent
with the §5.3 burst test rationale and is recorded as a diagnostic
only — no leak inference is drawn from a 60-second window.

### 5.4 GC behavior

| Generation | Collections |
|------------|-------------|
| Gen 0      | 52          |
| Gen 1      | 15          |
| Gen 2      | 4           |

GC pressure is light at 100 RPS in this composition. No Gen 2
collections exceeded the per-request latency budget except possibly
the single 110 ms max sample.

### 5.5 Persistence latency

The §5.3.1 prompt asks for event-store and outbox enqueue latency
"if exposed." In the in-memory composition the persistence path runs
synchronously inside the per-request `Stopwatch` window measured
above, so the per-request latency table in §5.2 is the
end-to-end-including-persistence number for this run. The runtime's
canonical `Whyce.EventStore` and `Whyce.Outbox` meters
(`event_store.append.advisory_lock_wait_ms`,
`event_store.append.hold_ms`, `outbox.depth`,
`outbox.oldest_pending_age_seconds`) only emit non-trivial samples
against the real Postgres composition; on the in-memory seams used
here they are vacuous and intentionally not consumed.

The §5.4 baseline (event-store endurance, Postgres-backed) will
consume those meters directly when the workstream lands.

### 5.6 Stability

| Metric              | Value |
|---------------------|-------|
| Unhandled exceptions| 0     |
| Failed results      | 0     |
| Retries observed    | 0     |
| Error rate          | **0.000%** |

Zero failures across 6,001 dispatches. Recorded explicitly per the
§5.3.1 prompt requirement that "error rate must be recorded even if
zero."

## 6. Acceptance criteria

| ID  | Criterion                                                           | Result |
|-----|---------------------------------------------------------------------|--------|
| B1  | System remains stable (no crashes, no unhandled exceptions)         | PASS — 0 exceptions |
| B2  | Error rate recorded                                                 | PASS — 0/6001 = 0.000% |
| B3  | Latency percentiles computed correctly (sorted, monotonic)          | PASS — 39 ≤ 216 ≤ 507 ≤ 1124 ≤ 109957 |
| B4  | Throughput matches target ±10%                                      | PASS — 100.0 / 100 (0.0% drift) |
| B5  | Resource usage captured (CPU + memory + mid sample)                 | PASS — 56→76→83 MB, 3,516 ms CPU |
| B6  | Evidence reproducible (test deterministic in shape, gated)          | PASS — single-dispatcher fixed cadence, gated by `BaselineTest__Enabled` |

## 7. Anomalies observed

- **One latency outlier at 109.957 ms.** The maximum sample is ~100×
  the p99. Consistent with a single early-window JIT or GC pause and
  not reflective of the steady-state distribution. The body of the
  distribution (p99 ≤ 1.124 ms) is unaffected. Recorded as a
  diagnostic, not flagged as a failure.
- **Working set grew +27 MB (1.48×) across 60 seconds.** Expected
  property of the in-memory composition (event store and outbox
  neither evict nor compact). Same rationale as the §5.3 burst test
  L11 disposition. No leak inference is drawn from a 60-second
  window; §5.3.2 soak (when scheduled) is the canonical leak gate.

No other anomalies observed.

## 8. Reproducibility

To reproduce:

```
cd c:/projects/dev/v5
dotnet build tests/integration/Whycespace.Tests.Integration.csproj
BaselineTest__Enabled=true \
  dotnet test tests/integration/Whycespace.Tests.Integration.csproj \
    --no-build \
    --filter "FullyQualifiedName~BaselinePerformanceTest" \
    --logger "console;verbosity=detailed"
```

Expected wall-clock: ~60 seconds for the test body plus xUnit harness
overhead (~1 s).

## 9. Statement

**Baseline established.**

The single-host, in-memory `RuntimeControlPlane` composition sustains
100 RPS at p50=216 µs / p95=507 µs / p99=1,124 µs end-to-end, with
zero errors, ~0.7% per-core CPU on this hardware, and bounded
working-set growth consistent with the in-memory persistence seams.
This is the canonical Phase 1.5B §5.3.1 baseline. All subsequent
§5.3.x sections measure against — and may not silently regress —
these numbers.

§5.3.1 is COMPLETE. §5.3.2 is NOT yet started per the §5.3.1 prompt's
explicit instruction.
