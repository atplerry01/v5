using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.MaintenanceResource;

public sealed class MaintenanceResourceProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.maintenance-resource";

    public string[] EventTypes =>
    [
        "whyce.business.resource.maintenance-resource.created",
        "whyce.business.resource.maintenance-resource.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IMaintenanceResourceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new MaintenanceResourceReadModel
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
