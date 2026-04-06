using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Synchronization;

public sealed class SynchronizationProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.synchronization";

    public string[] EventTypes =>
    [
        "whyce.business.integration.synchronization.created",
        "whyce.business.integration.synchronization.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISynchronizationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SynchronizationReadModel
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
