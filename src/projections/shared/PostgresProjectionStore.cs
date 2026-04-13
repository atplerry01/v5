using System.Diagnostics.Metrics;
using System.Text.Json;
using Npgsql;

namespace Whycespace.Projections.Shared;

/// <summary>
/// Generic Postgres projection store. Provides instrumented connection acquisition,
/// JSONB state Load, and idempotent Upsert for any read-model type.
///
/// Idempotency guarantee: the upsert WHERE clause guards on last_event_id —
/// if the same event is replayed (consumer rewind, retry storm), the row is
/// a no-op. No double-increments, no state re-merge.
///
/// SQL is sourced from ProjectionSqlProvider — no inline SQL in this class.
/// </summary>
public sealed class PostgresProjectionStore<TState> where TState : class
{
    private const string PoolName = "projections";

    private static readonly Meter PoolMeter = new("Whycespace.Postgres", "1.0");
    private static readonly Counter<long> PoolAcquisitions =
        PoolMeter.CreateCounter<long>("postgres.pool.acquisitions");
    private static readonly Counter<long> PoolAcquisitionFailures =
        PoolMeter.CreateCounter<long>("postgres.pool.acquisition_failures");

    private readonly NpgsqlDataSource _dataSource;
    private readonly string _aggregateType;
    private readonly string _loadSql;
    private readonly string _upsertSql;

    public PostgresProjectionStore(
        NpgsqlDataSource dataSource,
        string schema,
        string table,
        string aggregateType)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
        _aggregateType = aggregateType;
        _loadSql = ProjectionSqlProvider.GetLoadSql(schema, table);
        _upsertSql = ProjectionSqlProvider.GetUpsertSql(schema, table);
    }

    public async Task<TState?> LoadAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        await using var conn = await OpenInstrumentedAsync();
        await using var cmd = new NpgsqlCommand(_loadSql, conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var json = reader.GetString(0);
        return JsonSerializer.Deserialize<TState>(json);
    }

    public async Task UpsertAsync(
        Guid aggregateId,
        TState state,
        string lastEventType,
        Guid eventId,
        long eventVersion,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        var stateJson = JsonSerializer.Serialize(state);

        await using var conn = await OpenInstrumentedAsync();
        await using var cmd = new NpgsqlCommand(_upsertSql, conn);
        cmd.Parameters.AddWithValue("aggId", aggregateId);
        cmd.Parameters.AddWithValue("aggType", _aggregateType);
        cmd.Parameters.AddWithValue("eventVersion", eventVersion);
        cmd.Parameters.AddWithValue("state", stateJson);
        cmd.Parameters.AddWithValue("lastEventId", eventId);
        cmd.Parameters.AddWithValue("eventType", lastEventType);
        cmd.Parameters.AddWithValue("corrId", correlationId);

        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }

    private async Task<NpgsqlConnection> OpenInstrumentedAsync()
    {
        try
        {
            var conn = await _dataSource.OpenConnectionAsync();
            PoolAcquisitions.Add(1, new KeyValuePair<string, object?>("pool", PoolName));
            return conn;
        }
        catch (Exception ex)
        {
            PoolAcquisitionFailures.Add(1,
                new KeyValuePair<string, object?>("pool", PoolName),
                new KeyValuePair<string, object?>("reason", ex.GetType().Name));
            throw;
        }
    }
}
