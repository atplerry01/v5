using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Allocation;

public sealed class AllocationProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.allocation";

    public string[] EventTypes =>
    [
        "whyce.business.execution.allocation.created",
        "whyce.business.execution.allocation.updated"
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
