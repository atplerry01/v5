using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic.Forecast;

public sealed class ForecastProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic.forecast";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic.forecast.created",
        "whyce.intelligence.economic.forecast.updated"
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
