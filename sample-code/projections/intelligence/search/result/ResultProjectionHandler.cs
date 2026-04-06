using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Search.Result;

public sealed class ResultProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.search.result";

    public string[] EventTypes =>
    [
        "whyce.intelligence.search.result.created",
        "whyce.intelligence.search.result.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IResultViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ResultReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
