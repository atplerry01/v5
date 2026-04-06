using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Scheduler.Availability;

public sealed class AvailabilityProjectionHandler
{
    public string ProjectionName => "whyce.business.scheduler.availability";

    public string[] EventTypes =>
    [
        "whyce.business.scheduler.availability.created",
        "whyce.business.scheduler.availability.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAvailabilityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AvailabilityReadModel
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
