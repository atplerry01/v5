using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Outcome;

public sealed class OutcomeProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.outcome";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.outcome.created",
        "whyce.intelligence.simulation.outcome.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IOutcomeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new OutcomeReadModel
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
