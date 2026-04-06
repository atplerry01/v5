using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Resource.Equipment;

public sealed class EquipmentProjectionHandler
{
    public string ProjectionName => "whyce.business.resource.equipment";

    public string[] EventTypes =>
    [
        "whyce.business.resource.equipment.created",
        "whyce.business.resource.equipment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEquipmentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EquipmentReadModel
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
