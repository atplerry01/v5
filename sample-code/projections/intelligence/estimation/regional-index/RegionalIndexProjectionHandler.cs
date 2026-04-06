using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Estimation.RegionalIndex;

public sealed class RegionalIndexProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.estimation.regional-index";

    public string[] EventTypes =>
    [
        "whyce.intelligence.estimation.regional-index.created",
        "whyce.intelligence.estimation.regional-index.updated"
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
