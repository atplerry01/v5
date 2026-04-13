# New Rule Capture: SQL NOW() in Infrastructure Adapters

**CLASSIFICATION:** determinism
**SOURCE:** Phase 2 Entry Determinism Audit (2026-04-13)
**SEVERITY:** S3 (formatting/documentation — rule clarification, not a new violation category)

## DESCRIPTION

During the Phase 2 entry determinism audit, SQL `NOW()` was found in multiple infrastructure adapters (event store, outbox, projections, idempotency store, Kafka publisher). These needed explicit classification under $9 (determinism) to confirm whether they are violations or acceptable infrastructure timestamps.

## FINDING

All SQL `NOW()` usages in infrastructure adapters are **storage-layer operational timestamps**, not business/replay-significant timestamps. They are acceptable because:

1. **Projection `projected_at` / `created_at`:** Records when the read model was materialized. Projections are rebuildable from events — these timestamps reset on rebuild and are not consumed by domain logic.

2. **Event store `created_at`:** Physical write timestamp. The deterministic business timestamp is carried inside the event envelope (set via IClock at the runtime seam). `created_at` is never used for replay, ordering, or business logic.

3. **Outbox `created_at` / `published_at` / `next_retry_at`:** Infrastructure scheduling for the relay publisher. Used for ordering pickup batches and exponential backoff. Not business time.

4. **Idempotency key `created_at`:** Infrastructure record of when a claim was made. Not business time.

## PROPOSED_RULE

> **$9 Addendum — SQL Server-Side Timestamps:**
> SQL `NOW()` / `CURRENT_TIMESTAMP` is acceptable in infrastructure adapter SQL for storage-layer operational timestamps (`created_at`, `projected_at`, `published_at`, `next_retry_at`) provided:
> - The timestamp is never consumed by domain logic, event replay, chain integrity, or deterministic ID generation
> - Business-significant time is always supplied from the IClock seam via parameterized values
> - The usage is confined to the platform/host adapter layer (never in domain or engine SQL)
>
> If any new SQL timestamp is introduced that could affect replay correctness or business logic, it must use a parameterized timestamp from IClock instead of `NOW()`.

## EVIDENCE

See: `claude/audits/determinism-phase2-entry.audit.output.md` — inventory items #10-#21.
