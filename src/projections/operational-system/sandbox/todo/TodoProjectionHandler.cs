using System.Text.Json;
using Npgsql;
using Whyce.Runtime.EventFabric;
using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;
using EnvelopeProjectionHandler = Whyce.Runtime.Projection.IProjectionHandler;
namespace Whyce.Projections.OperationalSystem.Sandbox.Todo;

/// <summary>
/// Materializes the Todo read model in Postgres by merging events into a per-aggregate
/// state row. Owns the write to projection_operational_sandbox_todo.todo_read_model —
/// the generic projection writer is suppressed for handled events so this handler is
/// the single source of truth for the row's `state` JSONB.
/// </summary>
public sealed class TodoProjectionHandler :
    EnvelopeProjectionHandler,
    IProjectionHandler<TodoCreatedEventSchema>,
    IProjectionHandler<TodoUpdatedEventSchema>,
    IProjectionHandler<TodoCompletedEventSchema>
{
    private const string Schema = "projection_operational_sandbox_todo";
    private const string Table = "todo_read_model";
    private const string AggregateType = "Todo";

    private readonly string _connectionString;

    public TodoProjectionHandler(string connectionString)
    {
        _connectionString = connectionString;
    }

    public Whyce.Runtime.Projection.ProjectionExecutionPolicy ExecutionPolicy => Whyce.Runtime.Projection.ProjectionExecutionPolicy.Inline;

    public Task HandleAsync(EventEnvelope envelope) => envelope.Payload switch
    {
        TodoCreatedEventSchema created => HandleAsync(created),
        TodoUpdatedEventSchema updated => HandleAsync(updated),
        TodoCompletedEventSchema completed => HandleAsync(completed),
        _ => throw new InvalidOperationException(
            $"TodoProjectionHandler received unmatched event type: {envelope.Payload.GetType().Name}. " +
            $"EventId={envelope.EventId}, EventType={envelope.EventType}.")
    };

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

        var sql = $"""
            INSERT INTO {Schema}.{Table}
                (aggregate_id, aggregate_type, current_version, state, last_event_type, correlation_id, projected_at, created_at)
            VALUES
                (@aggId, @aggType, 1, @state::jsonb, @eventType, @corrId, NOW(), NOW())
            ON CONFLICT (aggregate_id) DO UPDATE SET
                current_version = {Schema}.{Table}.current_version + 1,
                state = @state::jsonb,
                last_event_type = @eventType,
                projected_at = NOW()
            """;

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("aggId", aggregateId);
        cmd.Parameters.AddWithValue("aggType", AggregateType);
        cmd.Parameters.AddWithValue("state", stateJson);
        cmd.Parameters.AddWithValue("eventType", lastEventType);
        cmd.Parameters.AddWithValue("corrId", Guid.Empty);

        await cmd.ExecuteNonQueryAsync();
    }
}
