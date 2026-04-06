using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Forecast;

public sealed class ForecastProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.forecast";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.forecast.created",
        "whyce.intelligence.simulation.forecast.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IForecastViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ForecastReadModel
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
