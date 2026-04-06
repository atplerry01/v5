using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Search.Query;

public sealed class QueryProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.search.query";

    public string[] EventTypes =>
    [
        "whyce.intelligence.search.query.created",
        "whyce.intelligence.search.query.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IQueryViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new QueryReadModel
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
