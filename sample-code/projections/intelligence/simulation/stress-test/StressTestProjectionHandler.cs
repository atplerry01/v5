using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.StressTest;

public sealed class StressTestProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.stress-test";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.stress-test.created",
        "whyce.intelligence.simulation.stress-test.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStressTestViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StressTestReadModel
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
