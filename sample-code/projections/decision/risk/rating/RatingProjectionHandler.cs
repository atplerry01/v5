using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Rating;

public sealed class RatingProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.rating";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.rating.created",
        "whyce.decision.risk.rating.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRatingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RatingReadModel
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
