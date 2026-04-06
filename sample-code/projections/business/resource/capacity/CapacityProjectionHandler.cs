using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.Capacity;

public sealed class CapacityProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.capacity";

    public string[] EventTypes =>
    [
        "whyce.business.resource.capacity.created",
        "whyce.business.resource.capacity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICapacityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CapacityReadModel
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
