using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Index.RegionalIndex;

public sealed class RegionalIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.index.regional-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.index.regional-index.created",
        "whyce.intelligence.index.regional-index.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRegionalIndexViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RegionalIndexReadModel
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
