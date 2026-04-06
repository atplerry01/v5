using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Provider;

public sealed class ProviderProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.provider";

    public string[] EventTypes =>
    [
        "whyce.business.integration.provider.created",
        "whyce.business.integration.provider.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IProviderViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ProviderReadModel
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
