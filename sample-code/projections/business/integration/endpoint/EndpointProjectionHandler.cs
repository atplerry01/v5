using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Endpoint;

public sealed class EndpointProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.endpoint";

    public string[] EventTypes =>
    [
        "whyce.business.integration.endpoint.created",
        "whyce.business.integration.endpoint.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEndpointViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EndpointReadModel
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
