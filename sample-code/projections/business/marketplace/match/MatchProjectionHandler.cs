using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Marketplace.Match;

public sealed class MatchProjectionHandler
{
    public string ProjectionName => "whyce.business.marketplace.match";

    public string[] EventTypes =>
    [
        "whyce.business.marketplace.match.created",
        "whyce.business.marketplace.match.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMatchViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MatchReadModel
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
