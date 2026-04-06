using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Operational.Sandbox.Todo;

public sealed class TodoProjectionHandler
{
    public string ProjectionName => "whyce.operational.sandbox.todo";

    public string[] EventTypes =>
    [
        "whyce.operational.sandbox.todo.created",
        "whyce.operational.sandbox.todo.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITodoViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TodoReadModel
        {
            TodoId = @event.AggregateId.ToString(),
            Title = string.Empty,
            Description = string.Empty,
            Status = "Active",
            Priority = 0,
            CreatedAt = @event.Timestamp,
            UpdatedAt = @event.Timestamp
        };

        await repository.SaveAsync(model, ct);
    }
}
