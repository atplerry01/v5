namespace Whyce.Projections.Shared;

/// <summary>
/// Externalizes all projection SQL from the store. Single source of truth
/// for the SELECT/UPSERT statements used by PostgresProjectionStore.
/// </summary>
public static class ProjectionSqlProvider
{
    public static string GetLoadSql(string schema, string table) =>
        $"SELECT state FROM {schema}.{table} WHERE aggregate_id = @id LIMIT 1";

    public static string GetUpsertSql(string schema, string table) =>
        $"""
        INSERT INTO {schema}.{table}
            (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, projected_at, created_at)
        VALUES
            (@aggId, @aggType, 1, @state::jsonb, @lastEventId, @eventType, @corrId, NOW(), NOW())
        ON CONFLICT (aggregate_id) DO UPDATE SET
            current_version = {schema}.{table}.current_version + 1,
            state = @state::jsonb,
            last_event_id = @lastEventId,
            last_event_type = @eventType,
            projected_at = NOW()
        WHERE {schema}.{table}.last_event_id IS DISTINCT FROM @lastEventId
        """;
}
