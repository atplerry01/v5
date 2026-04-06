using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Comparison;

public sealed class ComparisonProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.comparison";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.comparison.created",
        "whyce.intelligence.simulation.comparison.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IComparisonViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ComparisonReadModel
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
