# Runtime Retry-Site Inventory (R1 Batch 6 baseline)

**Captured:** 2026-04-19
**Purpose:** R1 baseline for R2 resilience work. Every retry / backoff / polling-delay site currently in the runtime and adjacent layers, with its discipline, retention, and R2 disposition.

R1 does NOT introduce new retry primitives (that is R2's job). This inventory exists so R2 can replace ad-hoc retry with the canonical `IRetryExecutor` (per decision D1 — Custom retry on `IClock` + `IRandomProvider`) without missing sites.

---

## Legend

- **Discipline:**
  - `Bounded-graduated` — explicit MaxAttempts + exponential backoff
  - `Fixed-poll` — periodic Task.Delay between work polls (background worker)
  - `Bare-delay` — one-off Task.Delay inside a reactive path
  - `None` — no retry, rely on caller / dead-letter
- **Determinism:** Whether the delay source is `IClock`-compatible (replay-safe).
- **R2 disposition:** What R2 will replace this with.

---

## 1. Canonical retry policy type (SHARED CONTRACT)

- **File:** [`src/shared/contracts/runtime/RetryPolicy.cs`](../../src/shared/contracts/runtime/RetryPolicy.cs)
- **Shape:** `MaxAttempts` (default 3) + `InitialDelayMs` (default 200ms) + `GetDelayMs(attempt)` exponential.
- **Outcomes:** `Success` / `Exhausted` / `PermanentFailure` / `Cancelled`.
- **Permanent-failure heuristic:** string-based on `"validation" / "not found" / "already exists" / "WHYCEPOLICY"` — fragile, to be replaced by `RuntimeFailureCategory` classification in R2.
- **Determinism:** OK — takes no clock or RNG of its own; caller supplies them.
- **R2 disposition:** Replaced by `IRetryExecutor` contract (new) + typed `RuntimeFailureCategory` predicates. `RetryPolicy` stays as a data record of `MaxAttempts`/`InitialDelayMs` but the state machine moves to the executor.

## 2. PostToLedgerStep — the one live business retry

- **File:** [`src/engines/T1M/domains/economic/transaction/steps/PostToLedgerStep.cs`](../../src/engines/T1M/domains/economic/transaction/steps/PostToLedgerStep.cs)
- **Discipline:** Bounded-graduated. `new RetryPolicy { MaxAttempts = 3, InitialDelayMs = 200 }`.
- **Flow:** workflow step retries ledger posting with 0ms / 200ms / 400ms delays; permanent-failure detection via `RetryPolicy.IsPermanentFailure`; exhausted → recovery queue if `_recoveryQueue is not null`.
- **Determinism:** `await Task.Delay(delayMs, cancellationToken)` — wall-clock. **NOT** replay-safe.
- **R2 disposition:** Rewritten on top of `IRetryExecutor` with `IRandomProvider` jitter; retry decisions keyed on `RuntimeFailureCategory`. Permanent-failure detection stops being string-based.

## 3. PayoutRetrySpecification — domain retry rule

- **File:** `src/domain/economic-system/revenue/payout/specification/PayoutRetrySpecification.cs`
- **Discipline:** Domain specification encoding "is this payout eligible for retry?" — NOT a retry loop itself, a decision rule consulted by a workflow.
- **Determinism:** Pure function of aggregate state. Replay-safe.
- **R2 disposition:** Unchanged. R2 adds the retry LOOP primitive around the specification; the specification itself stays.

## 4. ReconciliationLifecycleHandler — bare delay

- **File:** [`src/runtime/event-fabric/ReconciliationLifecycleHandler.cs:149`](../../src/runtime/event-fabric/ReconciliationLifecycleHandler.cs)
- **Discipline:** Bare-delay. `await Task.Delay(100, cancellationToken)` — single 100ms delay, appears to mitigate a read-after-write race.
- **Determinism:** NOT replay-safe (wall-clock).
- **R2 disposition:** Re-examine — if it's a race mitigation, replace with a deterministic signal (outbox read-through, consumer watermark check). If it's a brief retry, fold into `IRetryExecutor` semantics.

## 5. Background worker polling loops (Fixed-poll)

All periodic workers follow the same shape: `try { await Task.Delay(_interval, stoppingToken); } catch {}` inside their `ExecuteAsync` loop. These are poll intervals, not retries of a specific operation.

| Worker | Interval | File |
|---|---|---|
| `SystemLockExpirySchedulerWorker` | configurable | `src/platform/host/adapters/SystemLockExpirySchedulerWorker.cs:90` |
| `SanctionExpirySchedulerWorker` | configurable | `src/platform/host/adapters/SanctionExpirySchedulerWorker.cs:112` |
| `RiskExposureEnforcementWorker` | 1s outer / 1s inner | `src/platform/host/adapters/RiskExposureEnforcementWorker.cs:169,179` |
| `OutboxDepthSampler` | configurable | `src/platform/host/adapters/OutboxDepthSampler.cs:136` |
| `KafkaOutboxPublisher` | `_pollInterval` | `src/platform/host/adapters/KafkaOutboxPublisher.cs:112,121` |
| `GenericKafkaProjectionConsumerWorker` | 1s or 5s (on error) | `src/platform/host/adapters/GenericKafkaProjectionConsumerWorker.cs:395,400` |

**Discipline:** All use `Task.Delay` with a `CancellationToken` — graceful shutdown works, but `IClock` is not consulted. These are wall-clock delays between polls, which is acceptable for polling workers (poll cadence is not a replay-sensitive decision).

**R2 disposition:** Leave fixed-poll intervals alone (they don't affect replay). Any of these that couple to a retry of a specific message (`KafkaOutboxPublisher`, `GenericKafkaProjectionConsumerWorker` on exception) should route through `IRetryExecutor` so retry counts, backoff, and dead-letter routing are canonical.

## 6. IdempotencyStore claim-release — rollback, not retry

- **File:** [`src/runtime/middleware/post-policy/IdempotencyMiddleware.cs`](../../src/runtime/middleware/post-policy/IdempotencyMiddleware.cs)
- **Discipline:** Not a retry — it's a claim + conditional release. Failed inner pipeline releases the claim so a subsequent retry (by the caller) isn't blocked.
- **R2 disposition:** Extended in R2 with idempotency expiry/retention rules (spec §8) so stale claims don't accumulate. The middleware wiring itself is unchanged.

## 7. Execution lock acquire — one-shot, no retry

- **File:** [`src/runtime/control-plane/RuntimeControlPlane.cs`](../../src/runtime/control-plane/RuntimeControlPlane.cs) + `src/platform/host/adapters/RedisExecutionLockProvider.cs`
- **Discipline:** `TryAcquireAsync` — single attempt, no retry. Failure returns `CommandResult.Failure("execution_lock_unavailable", DependencyUnavailable)`.
- **R2 disposition:** Keep one-shot semantics (retrying a lock acquire in-process defeats multi-instance safety). R2 adds lease renewal for long-running commands and crash-safe recovery (D3 backend decision pending).

## 8. OPA / WHYCEPOLICY evaluator — no retry today

- **File:** `src/platform/host/adapters/OpaPolicyEvaluator.cs`
- **Discipline:** Single HTTP call to OPA; failure surfaces as a deny or exception. No retry on transient 5xx.
- **R2 disposition:** **This is where POL-FAIL-CLASS-01 lands.** R2 retry distinguishes `FAIL_CLOSED` (no retry, reject) from `DEFER` (bounded retry via `IRetryExecutor`) from `FAIL_OPEN` (only under audited degraded posture). Current behavior is effectively `FAIL_CLOSED` for all errors — correct default, but coarse.

## 9. Event fabric append / outbox / chain — no in-call retry

- **Files:** `src/platform/host/adapters/PostgresOutboxAdapter.cs`, `src/runtime/event-fabric/EventStoreService.cs`, `src/runtime/event-fabric/ChainAnchorService.cs`
- **Discipline:** Database calls with transactional semantics. Failure propagates up as exceptions, mapped to `RuntimeFailureCategory.PersistenceFailure` / `ConcurrencyConflict` / etc via `RuntimeExceptionMapper` (R1 Batch 3.5 wiring). No retry inside the fabric — that's intentional; the caller retries via the outbox relay pattern.
- **R2 disposition:** Unchanged at the fabric layer. `KafkaOutboxPublisher` gets DLQ routing (D2 decision: tiered `{topic}.retry` → `{topic}.dlq`).

---

## R2 Migration Summary

When R2 builds `IRetryExecutor`:

1. **PostToLedgerStep** (§2) is the reference consumer — migrate first.
2. **OPA evaluator** (§8) is the second — adds `PolicyEvaluationDeferred` classification.
3. **Worker poll loops** (§5) stay wall-clock except where they wrap a specific retryable operation (Kafka publish retry in `KafkaOutboxPublisher`).
4. **Bare delay** (§4) is re-examined case-by-case — may not be a retry at all.
5. **IdempotencyStore expiry** (§6) gains a retention sweep, separate from the retry executor.
6. **DLQ plumbing** (§9) lands on top of `KafkaOutboxPublisher`'s poll loop — `{topic}.retry` tier + `{topic}.dlq` terminal, per D2.

## Out-of-scope today (not R1 Batch 6)

- Circuit-breaker wrapping (D4: per-dependency, R2).
- Distributed lease renewal (D3, R2).
- Retry evidence / audit emission (R2 adds `RetryAttemptedEvent` / `RetryExhaustedEvent`).
- Jitter — awaiting `DeterministicRandomProvider` (implementation lands in R2 host composition).
