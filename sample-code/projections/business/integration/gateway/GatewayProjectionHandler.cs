using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Gateway;

public sealed class GatewayProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.gateway";

    public string[] EventTypes =>
    [
        "whyce.business.integration.gateway.created",
        "whyce.business.integration.gateway.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGatewayViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GatewayReadModel
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
