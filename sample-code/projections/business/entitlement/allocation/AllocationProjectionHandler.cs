using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Entitlement.Allocation;

public sealed class AllocationProjectionHandler
{
    public string ProjectionName => "whyce.business.entitlement.allocation";

    public string[] EventTypes =>
    [
        "whyce.business.entitlement.allocation.created",
        "whyce.business.entitlement.allocation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAllocationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AllocationReadModel
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
