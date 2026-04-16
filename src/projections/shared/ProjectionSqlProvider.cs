namespace Whycespace.Projections.Shared;

/// <summary>
/// Externalizes all projection SQL from the store. Single source of truth
/// for the SELECT/UPSERT statements used by PostgresProjectionStore.
/// </summary>
public static class ProjectionSqlProvider
{
    public static string GetLoadSql(string schema, string table) =>
        $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1";

    // P-IDEMPOTENCY-KEY-NOT-NULL-01 + P-VERSION-MONOTONE-01 remediation:
    //  - idempotency_key is now populated on both INSERT and UPDATE paths from
    //    the event envelope's EventId. The unique constraint on this column
    //    provides a second duplicate-delivery defence in addition to the
    //    aggregate_id primary key (P5 — PROJECTION IDEMPOTENCY).
    //  - current_version is now incremented server-side on UPDATE
    //    (`{table}.current_version + 1`) so it is strictly monotonic, rather
    //    than sourced from the intra-batch SequenceNumber which is always 0
    //    for single-event commands. First INSERT starts the stream at 1.
    //  - The legacy `current_version < @eventVersion` WHERE guard is dropped
    //    because duplicate detection is now done via the
    //    `last_event_id IS DISTINCT FROM @lastEventId` predicate alone. Replay
    //    of the same event is still a no-op (row untouched).
    public static string GetUpsertSql(string schema, string table) =>
        $"""
        INSERT INTO {schema}.{table}
            (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, idempotency_key, projected_at, created_at)
        VALUES
            (@aggId, @aggType, 1, @state::jsonb, @lastEventId, @eventType, @corrId, @idempKey, NOW(), NOW())
        ON CONFLICT (aggregate_id) DO UPDATE SET
            current_version = {schema}.{table}.current_version + 1,
            state = @state::jsonb,
            last_event_id = @lastEventId,
            last_event_type = @eventType,
            correlation_id = @corrId,
            idempotency_key = @idempKey,
            projected_at = NOW()
        WHERE {schema}.{table}.last_event_id IS DISTINCT FROM @lastEventId
        """;
}
