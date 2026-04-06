using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Adapter;

public sealed class AdapterProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.adapter";

    public string[] EventTypes =>
    [
        "whyce.business.integration.adapter.created",
        "whyce.business.integration.adapter.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAdapterViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AdapterReadModel
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
