using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.Operational.Sandbox.Todo;

public static class TodoCompletedHandler
{
    private const string Projection = "todo";

    public static async Task HandleAsync(ProjectionEvent @event, IProjectionStore store, CancellationToken ct)
    {
        var todoId = ExtractId(@event);
        if (todoId is null) return;

        var existing = await store.GetAsync<TodoReadModel>(Projection, todoId, ct);
        if (existing is null) return;

        await store.SetAsync(Projection, todoId, existing with
        {
            Status = "completed",
            CompletedAt = @event.Timestamp,
            UpdatedAt = @event.Timestamp
        }, ct);
    }

    private static string? ExtractId(ProjectionEvent e)
    {
        if (e.Payload is not JsonElement json)
        {
            var s = JsonSerializer.Serialize(e.Payload);
            json = JsonDocument.Parse(s).RootElement;
        }

        return json.TryGetProperty("TodoId", out var id) ? id.GetString() : null;
    }
}
