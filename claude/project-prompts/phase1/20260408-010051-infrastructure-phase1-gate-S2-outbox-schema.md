# TITLE
phase1-gate-S2 — Outbox schema upgrade: hoist event_id and aggregate_id columns

# CONTEXT
Phase 1 Hardening Pass, Task 2. The outbox table currently lacks event_id
and aggregate_id columns; the KafkaOutboxPublisher derives aggregate_id by
parsing the JSONB payload at publish time (TryExtractAggregateId). The
spec mandates removing this workaround.

Classification: infrastructure / phase1-hardening / outbox-schema
Domain: platform/host adapters; postgres outbox migration

# OBJECTIVE
Persist event_id + aggregate_id as discrete NOT NULL columns; populate at
enqueue; read directly at publish; delete the JSON parsing fallback.

# CONSTRAINTS
- $5: no architecture changes; outbox API surface unchanged.
- $9: deterministic IDs; no Guid.NewGuid.
- Migration must be backfill-safe for existing rows on dev_wip.
- Domain layer untouched ($7).

# EXECUTION STEPS
1. Create migration 004_outbox_event_aggregate_ids.sql:
   - ADD COLUMN event_id, aggregate_id (UUID, NULL initially)
   - Backfill: event_id = id; aggregate_id = Guid.Empty
   - SET NOT NULL on both
   - Index on aggregate_id
2. PostgresOutboxAdapter.EnqueueAsync:
   - Compute event_id = ComputeDeterministicId (same as outbox.id)
   - Extract aggregate_id from domain event via reflection on AggregateId
     property (handles both AggregateId struct and raw Guid shapes)
   - INSERT writes both columns
3. KafkaOutboxPublisher.PublishBatchAsync:
   - SELECT now reads event_id, aggregate_id
   - Headers populated from row directly
   - Delete TryExtractAggregateId helper and System.Text.Json import

# OUTPUT FORMAT
- 1 new migration
- 2 file edits (PostgresOutboxAdapter, KafkaOutboxPublisher)

# VALIDATION CRITERIA
- dotnet build src/platform/host succeeds
- Zero references to TryExtractAggregateId or JsonDocument in KafkaOutboxPublisher
- Migration applies cleanly to a fresh and an existing dev DB
