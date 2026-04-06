using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Search.Index;

public sealed class IndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.search.index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.search.index.created",
        "whyce.intelligence.search.index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IndexReadModel
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
