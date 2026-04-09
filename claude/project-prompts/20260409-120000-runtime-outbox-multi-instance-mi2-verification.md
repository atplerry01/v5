# PHASE 1.5 §5.2.5 MI-2 — OUTBOX MULTI-INSTANCE SAFETY (VERIFICATION)

## CLASSIFICATION
- classification: runtime-infrastructure
- context: outbox-publishing
- domain: multi-instance-safety
- phase: 1.5 §5.2.5 MI-2
- mode: VERIFICATION (Path A) — not implementation

## CONTEXT

The original prompt mandated a claim-column based design (`status='claimed'`,
`claimed_by`, `claimed_at`, reclaim sweeper) to guarantee exactly-once publish
across N runtime instances. Pre-execution discovery established that
`KafkaOutboxPublisher.PublishBatchAsync` already provides this guarantee
via the canonical Postgres pattern:

  - `SELECT ... FOR UPDATE SKIP LOCKED` inside an open transaction
  - row locks held across the Kafka `ProduceAsync` and the
    `UPDATE status='published'`
  - `COMMIT` is the irrevocable publish boundary
  - crash before COMMIT → automatic lock release → row reverts to
    `pending` → next worker re-picks it

Layering a parallel claim-column mechanism on top of an already-correct
implementation would violate $5 (Anti-Drift) and $15 (Architecture priority).

## DECISION (LOCKED BY USER)

Treat MI-2 as a verification task. No schema changes. No new claim mechanism.
Prove the existing guarantee with tests, and document the rationale inline
on `PublishBatchAsync` to prevent future drift toward the claim-column design.

## OBJECTIVE

Produce evidence that the existing `FOR UPDATE SKIP LOCKED` + tx-scoped
publish + commit pattern delivers exactly-once publish under:
  1. Multi-instance concurrent polling
  2. Worker crash between row claim and commit
  3. High-concurrency (N workers, M rows) saturation

## CONSTRAINTS

- DO NOT add schema fields
- DO NOT change `KafkaOutboxPublisher` behavior
- DO NOT introduce a reclaim sweeper
- DO NOT introduce new patterns
- Tests MUST be Postgres-gated and skip silently when
  `Postgres__TestConnectionString` is unset (mirrors
  `PostgresEventStoreConcurrencyTest`)
- Determinism ($9): no client-clock comparisons, no `Guid.NewGuid` in
  domain/runtime; tests may use `Guid.NewGuid` strictly for isolation tags

## EXECUTION STEPS

1. Add an integration test class
   `OutboxMultiInstanceSafetyTest` under
   `tests/integration/platform/host/adapters/` that exercises the SQL
   contract directly (raw `NpgsqlConnection`) so the proof targets the
   exact seam that owns the guarantee.
2. Implement three test cases:
   - `Multi_Instance_Workers_Publish_Each_Row_Exactly_Once`
   - `Crash_Before_Commit_Releases_Row_For_Survivor_To_Reprocess`
   - `High_Concurrency_N_Workers_M_Rows_No_Duplicates_No_Loss`
3. Add a "WHY" documentation block to
   `KafkaOutboxPublisher.PublishBatchAsync` explaining: row-lock
   ownership, tx-scoped publish, automatic crash recovery, and why
   claim columns are not required.
4. Run guards ($1a) and audits ($1b). Capture any new rules ($1c).

## OUTPUT FORMAT

Implementation summary, files modified, test list, audit results, evidence
of exactly-once guarantee.

## VALIDATION CRITERIA

- All three tests pass against a live Postgres instance and skip cleanly
  when the env var is unset.
- `KafkaOutboxPublisher.PublishBatchAsync` carries the WHY block.
- No schema migrations added.
- No production behavior changes.
- Guards and audits clean (or new-rules captured per $1c).
