using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Capacity.AllocationForecast;

public sealed class AllocationForecastProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.capacity.allocation-forecast";

    public string[] EventTypes =>
    [
        "whyce.intelligence.capacity.allocation-forecast.created",
        "whyce.intelligence.capacity.allocation-forecast.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAllocationForecastViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AllocationForecastReadModel
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
