using System.Diagnostics.Metrics;
using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.Operational.Sandbox.Todo;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Projections.Operational.Sandbox.Todo;

/// <summary>
/// Materializes the Todo read model in Postgres by merging events into a per-aggregate
/// state row. Owns the write to projection_operational_sandbox_todo.todo_read_model —
/// the generic projection writer is suppressed for handled events so this handler is
/// the single source of truth for the row's `state` JSONB.
///
/// Implements the shared envelope-based projection handler contract so this file does
/// not depend on src/runtime/**. Closes dependency-graph guard violation DG-R7-01.
/// </summary>
public sealed class TodoProjectionHandler :
    IEnvelopeProjectionHandler,
    IProjectionHandler<TodoCreatedEventSchema>,
    IProjectionHandler<TodoUpdatedEventSchema>,
    IProjectionHandler<TodoCompletedEventSchema>
{
    private const string Schema = "projection_operational_sandbox_todo";
    private const string Table = "todo_read_model";
    private const string AggregateType = "Todo";
    private const string PoolName = "projections";

    // phase1.5-S5.2.2 / KC-4 (PROJECTIONS-POOL-01): mirror of the
    // host-adapters PostgresPoolMetrics meter, defined locally so the
    // projections assembly does not need to reference the host-adapters
    // layer (which would violate the dependency direction). Both
    // meter instances share the canonical "Whyce.Postgres" name, so
    // System.Diagnostics.Metrics listeners (OTel, Prometheus, etc.)
    // collapse them into a single logical surface — operators see
    // pool="projections" acquisitions on the same scrape as
    // pool="event-store" and pool="chain".
    private static readonly Meter PoolMeter = new("Whyce.Postgres", "1.0");
    private static readonly Counter<long> PoolAcquisitions =
        PoolMeter.CreateCounter<long>("postgres.pool.acquisitions");
    private static readonly Counter<long> PoolAcquisitionFailures =
        PoolMeter.CreateCounter<long>("postgres.pool.acquisition_failures");

    private readonly NpgsqlDataSource _dataSource;

    // Carries the current envelope's correlation id + event id into the
    // per-type HandleAsync overloads. Safe because ExecutionPolicy is Inline —
    // each envelope is fully processed before the next is dispatched.
    private Guid _currentCorrelationId = Guid.Empty;
    private Guid _currentEventId = Guid.Empty;

    // phase1.5-S5.2.2 / KC-4 (PROJECTIONS-POOL-01): take an
    // NpgsqlDataSource directly instead of a connection string. The
    // host bootstrap (TodoBootstrap.cs) unwraps the .Inner data
    // source from the singleton ProjectionsDataSource wrapper at the
    // construction seam — the projections assembly stays free of any
    // host-adapters reference.
    public TodoProjectionHandler(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
    }

    /// <summary>
    /// phase1.5-S5.2.2 / KC-4: instrumented connection acquisition.
    /// Mirrors PostgresPoolMetrics.OpenInstrumentedAsync from the
    /// host-adapters layer with identical metric names and tags so
    /// every projections-side acquisition is visible alongside the
    /// event-store and chain pools.
    /// </summary>
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

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    // phase1.5-S5.2.3 / TC-6 (PROJECTION-CT-CONTRACT-01): the envelope
    // handler now consumes the worker's stoppingToken and forwards it
    // through every per-type overload into LoadAsync / UpsertAsync so
    // a hung handler can be unblocked at the Postgres round-trip
    // without waiting for Kafka poll/session limits to intervene.
    public Task HandleAsync(IEventEnvelope envelope, CancellationToken cancellationToken = default)
    {
        _currentCorrelationId = envelope.CorrelationId;
        _currentEventId = envelope.EventId;
        return envelope.Payload switch
        {
            TodoCreatedEventSchema created => HandleAsync(created, cancellationToken),
            TodoUpdatedEventSchema updated => HandleAsync(updated, cancellationToken),
            TodoCompletedEventSchema completed => HandleAsync(completed, cancellationToken),
            _ => throw new InvalidOperationException(
                $"TodoProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public async Task HandleAsync(TodoCreatedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken) ?? new TodoReadModel { Id = e.AggregateId };
        state = state with { Title = e.Title, IsCompleted = false, Status = "active" };
        await UpsertAsync(e.AggregateId, state, "TodoCreatedEvent", cancellationToken);
    }

    public async Task HandleAsync(TodoUpdatedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken) ?? new TodoReadModel { Id = e.AggregateId };
        state = state with { Title = e.Title };
        await UpsertAsync(e.AggregateId, state, "TodoUpdatedEvent", cancellationToken);
    }

    public async Task HandleAsync(TodoCompletedEventSchema e, CancellationToken cancellationToken = default)
    {
        var state = await LoadAsync(e.AggregateId, cancellationToken) ?? new TodoReadModel { Id = e.AggregateId };
        state = state with { IsCompleted = true, Status = "completed" };
        await UpsertAsync(e.AggregateId, state, "TodoCompletedEvent", cancellationToken);
    }

    private async Task<TodoReadModel?> LoadAsync(Guid aggregateId, CancellationToken cancellationToken)
    {
        await using var conn = await OpenInstrumentedAsync();

        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {Schema}.{Table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        // phase1.5-S5.2.3 / TC-6: ExecuteReaderAsync + ReadAsync now
        // honor the worker stoppingToken.
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return null;

        var json = reader.GetString(0);
        return JsonSerializer.Deserialize<TodoReadModel>(json);
    }

    private async Task UpsertAsync(Guid aggregateId, TodoReadModel state, string lastEventType, CancellationToken cancellationToken)
    {
        var stateJson = JsonSerializer.Serialize(state);

        await using var conn = await OpenInstrumentedAsync();

        // phase1-gate-H7-H9-safe (#11, #12): write last_event_id and gate the
        // ON CONFLICT update on it. If the same event_id is replayed (consumer
        // rewind, retry storm) the WHERE clause becomes false and the row is
        // a no-op — current_version no longer double-increments and state is
        // not re-merged. The INSERT path also writes last_event_id so the
        // first apply is captured. Idempotency is per-event, not per-batch.
        var sql = $"""
            INSERT INTO {Schema}.{Table}
                (aggregate_id, aggregate_type, current_version, state, last_event_id, last_event_type, correlation_id, projected_at, created_at)
            VALUES
                (@aggId, @aggType, 1, @state::jsonb, @lastEventId, @eventType, @corrId, NOW(), NOW())
            ON CONFLICT (aggregate_id) DO UPDATE SET
                current_version = {Schema}.{Table}.current_version + 1,
                state = @state::jsonb,
                last_event_id = @lastEventId,
                last_event_type = @eventType,
                projected_at = NOW()
            WHERE {Schema}.{Table}.last_event_id IS DISTINCT FROM @lastEventId
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggId", aggregateId);
        cmd.Parameters.AddWithValue("aggType", AggregateType);
        cmd.Parameters.AddWithValue("state", stateJson);
        cmd.Parameters.AddWithValue("lastEventId", _currentEventId);
        cmd.Parameters.AddWithValue("eventType", lastEventType);
        cmd.Parameters.AddWithValue("corrId", _currentCorrelationId);

        // phase1.5-S5.2.3 / TC-6: UPSERT round-trip honors stoppingToken.
        await cmd.ExecuteNonQueryAsync(cancellationToken);
    }
}
