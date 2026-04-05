using Whyce.Shared.Contracts.Application.Todo;
using Whyce.Shared.Contracts.Events.Todo;
using Whyce.Shared.Contracts.Infrastructure.Projection;

namespace Whyce.Projections.OperationalSystem.Sandbox.Todo;

public sealed class TodoProjectionHandler :
    IProjectionHandler<TodoCreatedEventSchema>,
    IProjectionHandler<TodoUpdatedEventSchema>,
    IProjectionHandler<TodoCompletedEventSchema>
{
    private readonly IRedisClient _redis;

    public TodoProjectionHandler(IRedisClient redis)
    {
        _redis = redis;
    }

    public async Task HandleAsync(TodoCreatedEventSchema e)
    {
        var existing = await _redis.GetAsync<TodoReadModel>($"todo:{e.AggregateId}");
        if (existing is not null) return;

        await _redis.SetAsync($"todo:{e.AggregateId}", new TodoReadModel
        {
            Id = e.AggregateId,
            Title = e.Title,
            IsCompleted = false
        });
    }

    public async Task HandleAsync(TodoUpdatedEventSchema e)
    {
        var existing = await _redis.GetAsync<TodoReadModel>($"todo:{e.AggregateId}");
        if (existing is null) return;

        await _redis.SetAsync($"todo:{e.AggregateId}", existing with { Title = e.Title });
    }

    public async Task HandleAsync(TodoCompletedEventSchema e)
    {
        var existing = await _redis.GetAsync<TodoReadModel>($"todo:{e.AggregateId}");
        if (existing is null) return;

        await _redis.SetAsync($"todo:{e.AggregateId}", existing with { IsCompleted = true });
    }
}
