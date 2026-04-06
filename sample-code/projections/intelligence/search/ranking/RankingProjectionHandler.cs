using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Search.Ranking;

public sealed class RankingProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.search.ranking";

    public string[] EventTypes =>
    [
        "whyce.intelligence.search.ranking.created",
        "whyce.intelligence.search.ranking.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRankingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RankingReadModel
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
