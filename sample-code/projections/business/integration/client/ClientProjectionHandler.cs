using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Client;

public sealed class ClientProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.client";

    public string[] EventTypes =>
    [
        "whyce.business.integration.client.created",
        "whyce.business.integration.client.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IClientViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ClientReadModel
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
