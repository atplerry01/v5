using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Recommendation;

public sealed class RecommendationProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.recommendation";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.recommendation.created",
        "whyce.intelligence.simulation.recommendation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRecommendationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RecommendationReadModel
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
