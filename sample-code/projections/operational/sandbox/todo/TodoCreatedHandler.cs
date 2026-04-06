using System.Text.Json;
using Whycespace.Shared.Contracts.Projection;
using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.Operational.Sandbox.Todo;

public static class TodoCreatedHandler
{
    private const string Projection = "todo";

    public static async Task HandleAsync(ProjectionEvent @event, IProjectionStore store, CancellationToken ct)
    {
        var (todoId, title, description, assignedTo, priority) = ExtractFields(@event);
        if (todoId is null) return;

        var model = new TodoReadModel
        {
            TodoId = todoId,
            Title = title ?? "",
            Description = description ?? "",
            Status = "created",
            Priority = priority,
            AssignedTo = assignedTo,
            CreatedAt = @event.Timestamp,
            UpdatedAt = @event.Timestamp
        };

        await store.SetAsync(Projection, todoId, model, ct);
    }

    private static (string? todoId, string? title, string? description, string? assignedTo, int priority) ExtractFields(ProjectionEvent e)
    {
        if (e.Payload is not JsonElement json)
        {
            var s = JsonSerializer.Serialize(e.Payload);
            json = JsonDocument.Parse(s).RootElement;
        }

        return (
            json.TryGetProperty("TodoId", out var id) ? id.GetString() : null,
            json.TryGetProperty("Title", out var t) ? t.GetString() : null,
            json.TryGetProperty("Description", out var d) ? d.GetString() : null,
            json.TryGetProperty("AssignedToIdentityId", out var a) ? a.GetString() : null,
            json.TryGetProperty("Priority", out var p) ? p.GetInt32() : 0
        );
    }
}
