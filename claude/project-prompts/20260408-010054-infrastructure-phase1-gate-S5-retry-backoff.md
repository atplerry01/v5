# TITLE
phase1-gate-S5 — Exponential backoff for outbox retries

# CONTEXT
Phase 1 Hardening, Task 4. Failed outbox rows currently re-eligible on the
next 1-second poll → retry budget burns in seconds, broker hammered.

Classification: infrastructure / phase1-hardening / outbox-retry
Domain: platform/host adapters; postgres outbox migration

# OBJECTIVE
Add next_retry_at column; gate failed-row re-selection until backoff has
elapsed; backoff = 2^(attempt-1) seconds, capped at 300s.

# CONSTRAINTS
- $9: no client clock — backoff computed via NOW() server-side.
- $5: SELECT signature unchanged from caller perspective.

# EXECUTION STEPS
1. Migration 005_outbox_next_retry_at.sql: ADD COLUMN next_retry_at TIMESTAMPTZ
   + partial index on (status='failed').
2. SELECT in PublishBatchAsync: add `(next_retry_at IS NULL OR next_retry_at <= NOW())`.
3. RecordFailureAsync: write next_retry_at via `NOW() + (@backoff || ' seconds')::interval`.

# OUTPUT FORMAT
- 1 new migration
- 1 file edit (KafkaOutboxPublisher.cs)

# VALIDATION CRITERIA
- dotnet build succeeds
- No DateTime.UtcNow / Guid.NewGuid introduced
- Backoff cap = 300s
