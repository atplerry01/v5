using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Temporal.Clock;

public sealed class ClockProjectionHandler
{
    public string ProjectionName => "whyce.core.temporal.clock";

    public string[] EventTypes =>
    [
        "whyce.core.temporal.clock.created",
        "whyce.core.temporal.clock.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IClockViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ClockReadModel
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
