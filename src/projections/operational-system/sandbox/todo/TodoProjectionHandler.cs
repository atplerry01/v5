using System.Text.Json;
using Npgsql;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.EventFabric;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using Whyce.Shared.Contracts.Projection;

namespace Whyce.Projections.OperationalSystem.Sandbox.Todo;

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

    private readonly string _connectionString;

    // Carries the current envelope's correlation id + event id into the
    // per-type HandleAsync overloads. Safe because ExecutionPolicy is Inline —
    // each envelope is fully processed before the next is dispatched.
    private Guid _currentCorrelationId = Guid.Empty;
    private Guid _currentEventId = Guid.Empty;

    public TodoProjectionHandler(string connectionString)
    {
        _connectionString = connectionString;
    }

    public ProjectionExecutionPolicy ExecutionPolicy => ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(IEventEnvelope envelope)
    {
        _currentCorrelationId = envelope.CorrelationId;
        _currentEventId = envelope.EventId;
        return envelope.Payload switch
        {
            TodoCreatedEventSchema created => HandleAsync(created),
            TodoUpdatedEventSchema updated => HandleAsync(updated),
            TodoCompletedEventSchema completed => HandleAsync(completed),
            _ => throw new InvalidOperationException(
                $"TodoProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
                $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
        };
    }

    public async Task HandleAsync(TodoCreatedEventSchema e)
    {
        var state = await LoadAsync(e.AggregateId) ?? new TodoReadModel { Id = e.AggregateId };
        state = state with { Title = e.Title, IsCompleted = false, Status = "active" };
        await UpsertAsync(e.AggregateId, state, "TodoCreatedEvent");
    }

    public async Task HandleAsync(TodoUpdatedEventSchema e)
    {
        var state = await LoadAsync(e.AggregateId) ?? new TodoReadModel { Id = e.AggregateId };
        state = state with { Title = e.Title };
        await UpsertAsync(e.AggregateId, state, "TodoUpdatedEvent");
    }

    public async Task HandleAsync(TodoCompletedEventSchema e)
    {
        var state = await LoadAsync(e.AggregateId) ?? new TodoReadModel { Id = e.AggregateId };
        state = state with { IsCompleted = true, Status = "completed" };
        await UpsertAsync(e.AggregateId, state, "TodoCompletedEvent");
    }

    private async Task<TodoReadModel?> LoadAsync(Guid aggregateId)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(
            $"SELECT state FROM {Schema}.{Table} WHERE aggregate_id = @id LIMIT 1", conn);
        cmd.Parameters.AddWithValue("id", aggregateId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;

        var json = reader.GetString(0);
        return JsonSerializer.Deserialize<TodoReadModel>(json);
    }

    private async Task UpsertAsync(Guid aggregateId, TodoReadModel state, string lastEventType)
    {
        var stateJson = JsonSerializer.Serialize(state);

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

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

        await cmd.ExecuteNonQueryAsync();
    }
}
