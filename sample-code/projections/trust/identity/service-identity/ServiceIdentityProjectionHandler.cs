using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.ServiceIdentity;

public sealed class ServiceIdentityProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.service-identity";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.service-identity.created",
        "whyce.trust.identity.service-identity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IServiceIdentityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ServiceIdentityReadModel
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
