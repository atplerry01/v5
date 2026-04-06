using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.Utilization;

public sealed class UtilizationProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.utilization";

    public string[] EventTypes =>
    [
        "whyce.business.resource.utilization.created",
        "whyce.business.resource.utilization.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IUtilizationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new UtilizationReadModel
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
