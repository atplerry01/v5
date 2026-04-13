# Determinism Phase 2 Entry Audit

**Date:** 2026-04-13
**Scope:** Full repo determinism sweep for Phase 2 entry certification
**Verdict:** PASS

---

## Executive Summary

A comprehensive determinism audit was executed across `src/**` and `infrastructure/**` searching for all forbidden non-deterministic patterns. One product violation was found and fixed. All remaining hits are classified as allowed infrastructure seams, documentation references, or the canonical SystemClock implementation.

---

## Scope Searched

**Directories:** `src/**`, `infrastructure/data/**`

**Patterns searched:**
- `Guid.NewGuid()`
- `DateTime.UtcNow` / `DateTime.Now`
- `DateTimeOffset.UtcNow` / `DateTimeOffset.Now`
- `new Random(` / `Random.Shared` / `RNGCryptoServiceProvider`
- `NOW()` / `CURRENT_TIMESTAMP` / `clock_timestamp()` / `gen_random_uuid()` / `uuid_generate_v4()`

---

## Inventory Table

| # | File | Line | Match | Classification | Rationale | Action |
|---|------|------|-------|---------------|-----------|--------|
| 1 | `src/shared/contracts/common/ApiResponse.cs` | 46 | `DateTime.UtcNow` | **A: PRODUCT VIOLATION** | API response envelope used uncontrolled wall clock for response timestamps | **FIXED** — factory methods now accept `DateTimeOffset timestamp` from caller's IClock |
| 2 | `src/platform/host/composition/core/SystemClock.cs` | 7 | `DateTimeOffset.UtcNow` | B: ALLOWED SEAM | Canonical IClock implementation — the sole approved wall-clock access point | No change |
| 3 | `src/runtime/guards/DeterminismGuard.cs` | 10-11 | doc comment | B: ALLOWED SEAM | Documentation listing forbidden patterns | No change |
| 4 | `src/platform/host/health/RedisHealthSnapshotProvider.cs` | 20 | doc comment | B: ALLOWED SEAM | XML doc warning against DateTime.UtcNow | No change |
| 5 | `src/platform/host/adapters/PostgresOutboxAdapter.cs` | 29 | doc comment | B: ALLOWED SEAM | Code comment explaining prior fix | No change |
| 6 | `src/platform/host/adapters/OutboxDepthSnapshot.cs` | 36 | doc comment | B: ALLOWED SEAM | XML doc warning against DateTime.UtcNow | No change |
| 7 | `src/domain/constitutional-system/policy/decision/README.md` | 50 | doc text | B: ALLOWED SEAM | README documenting forbidden patterns | No change |
| 8 | `src/domain/constitutional-system/policy/decision/event/PolicyEvaluatedEvent.cs` | 10 | doc comment | B: ALLOWED SEAM | XML doc warning against forbidden APIs | No change |
| 9 | `src/domain/business-system/document/template/README.md` | 96 | doc text | B: ALLOWED SEAM | README documenting forbidden patterns | No change |
| 10 | `src/projections/shared/ProjectionSqlProvider.cs` | 17,23 | `NOW()` | B: ALLOWED INFRA | Projection `projected_at`/`created_at` are storage-layer timestamps recording when materialization occurred — not business/replay-significant. Projections are rebuildable read models. | No change — documented |
| 11 | `src/platform/host/adapters/PostgresProjectionWriter.cs` | 62,68 | `NOW()` | B: ALLOWED INFRA | Same as #10 — projection materialization timestamps | No change — documented |
| 12 | `src/platform/host/adapters/PostgresEventStoreAdapter.cs` | 228 | `NOW()` | B: ALLOWED INFRA | Event store `created_at` is the physical write timestamp. Business time is in the event envelope (deterministic via IClock). `created_at` is never used for replay or business logic. | No change — documented |
| 13 | `src/platform/host/adapters/PostgresOutboxAdapter.cs` | 121 | `NOW()` | B: ALLOWED INFRA | Outbox `created_at` is infrastructure bookkeeping for publisher ordering — not business time | No change |
| 14 | `src/platform/host/adapters/PostgresIdempotencyStoreAdapter.cs` | 44,94 | `NOW()` | B: ALLOWED INFRA | Idempotency key `created_at` is infrastructure record — not business/replay time | No change |
| 15 | `src/platform/host/adapters/KafkaOutboxPublisher.cs` | 209 | `NOW()` | B: ALLOWED INFRA | Retry gate comparison — infrastructure scheduling | No change |
| 16 | `src/platform/host/adapters/KafkaOutboxPublisher.cs` | 304 | `NOW()` | B: ALLOWED INFRA | `published_at` — infrastructure timestamp for when Kafka delivery succeeded | No change |
| 17 | `src/platform/host/adapters/KafkaOutboxPublisher.cs` | 322,331 | `NOW()` | B: ALLOWED INFRA | Exponential backoff scheduling. Comment at line 322 explicitly cites $9 for server-side time. | No change |
| 18 | `src/platform/host/adapters/OutboxDepthSampler.cs` | 24,152 | `NOW()` | B: ALLOWED INFRA | Observability SQL — measures outbox depth/age for health metrics | No change |
| 19 | `infrastructure/data/postgres/event-store/migrations/001_event_store.sql` | 8 | `DEFAULT NOW()` | B: ALLOWED INFRA | DDL default for `created_at` — only fires if INSERT omits the column. Application INSERT at #12 uses NOW() explicitly, same classification. | No change |
| 20 | `infrastructure/data/postgres/outbox/migrations/001_outbox.sql` | 8 | `DEFAULT NOW()` | B: ALLOWED INFRA | DDL default for outbox `created_at` | No change |
| 21 | `infrastructure/data/postgres/projections/*/001_projection.sql` | various | `DEFAULT NOW()` | B: ALLOWED INFRA | DDL defaults for projection `projected_at`/`created_at` | No change |

**Random/RNG:** Zero hits in `src/` and `infrastructure/`. Clean.
**Guid.NewGuid():** Zero product code hits. Two doc comment references only.

---

## Fixes Applied

### Fix 1: ApiResponse.cs — Eliminate DateTime.UtcNow (Classification A)

**Problem:** `ApiResponse.BuildMeta()` called `DateTime.UtcNow` directly, meaning every API response carried an uncontrolled wall-clock timestamp. This violates $9 (determinism) by embedding non-deterministic time in product output.

**Fix:**
- `src/shared/contracts/common/ApiResponse.cs`: Factory methods `Ok()` and `Fail()` now require a `DateTimeOffset timestamp` parameter. `BuildMeta()` formats the provided timestamp instead of calling `DateTime.UtcNow`.
- `src/platform/api/controllers/operational/sandbox/todo/TodoController.cs`: Injected `IClock` via constructor. All `ApiResponse.Ok()` / `ApiResponse.Fail()` calls pass `_clock.UtcNow`.
- `src/platform/api/controllers/operational/sandbox/kanban/KanbanController.cs`: Injected `IClock` via constructor. All `ApiResponse.Ok()` / `ApiResponse.Fail()` calls pass `_clock.UtcNow`.

**Architecture impact:** Minimal. IClock was already registered in the DI container (used by PostgresOutboxAdapter). Controllers are in the platform layer where IClock injection is permitted. No new domain dependencies introduced.

---

## Remaining Accepted Exceptions

All remaining `NOW()` / `DateTimeOffset.UtcNow` occurrences are classified as **B: ALLOWED INFRASTRUCTURE** for the following reasons:

1. **SystemClock.cs** — The sole canonical IClock implementation. Allowed by locked rules.
2. **SQL NOW() in adapters/projections** — All are storage-layer or infrastructure-scheduling timestamps:
   - Projection `projected_at`/`created_at`: records when materialization happened. Projections are rebuildable from events. Not replay-significant.
   - Event store `created_at`: physical write time. Business timestamps live in the event envelope (set via IClock). Not used for replay or business logic.
   - Outbox/idempotency `created_at`: infrastructure bookkeeping.
   - Kafka publisher `published_at`/`next_retry_at`: delivery and retry scheduling.
   - Outbox depth sampler: observability query.
3. **SQL DDL `DEFAULT NOW()`** — Column defaults in migration files. Same classification as their corresponding INSERT statements.
4. **Doc comments/READMEs** — References to forbidden patterns in documentation. Not executable code.

---

## Governance Rule Clarification

**SQL NOW() in infrastructure adapters:** Server-side `NOW()` is explicitly acceptable for storage-layer operational timestamps (projection materialization time, event physical write time, outbox scheduling, idempotency bookkeeping) because:
- These timestamps are never consumed by domain logic, event replay, or chain integrity
- Projections are rebuildable read models — their `projected_at` reflects materialization time, not business time
- Event business time is carried in the deterministic event envelope (set via IClock at the runtime seam)
- The Kafka outbox publisher code at line 322 already documents this decision: "via NOW() to honor $9 (no client clock)"

This clarification is captured as a new rule in `claude/new-rules/`.

---

## Blocking Items

None.

---

## Final Verdict: PASS

- Zero product-code determinism violations remain in scope
- All remaining hits are justified and documented above
- The single product violation (ApiResponse.cs) has been fixed with minimal, architecture-safe changes
- Build compiles with 0 errors after fix
- No new domain dependencies introduced into platform/host
- Doctrine clarification on SQL infrastructure timestamps captured
