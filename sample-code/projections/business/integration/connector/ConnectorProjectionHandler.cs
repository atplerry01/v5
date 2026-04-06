using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Connector;

public sealed class ConnectorProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.connector";

    public string[] EventTypes =>
    [
        "whyce.business.integration.connector.created",
        "whyce.business.integration.connector.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IConnectorViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ConnectorReadModel
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
