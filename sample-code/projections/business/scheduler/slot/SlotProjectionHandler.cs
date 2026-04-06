using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Scheduler.Slot;

public sealed class SlotProjectionHandler
{
    public string ProjectionName => "whyce.business.scheduler.slot";

    public string[] EventTypes =>
    [
        "whyce.business.scheduler.slot.created",
        "whyce.business.scheduler.slot.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISlotViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SlotReadModel
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
