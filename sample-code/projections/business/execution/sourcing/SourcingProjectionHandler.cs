using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Sourcing;

public sealed class SourcingProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.sourcing";

    public string[] EventTypes =>
    [
        "whyce.business.execution.sourcing.created",
        "whyce.business.execution.sourcing.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISourcingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SourcingReadModel
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
