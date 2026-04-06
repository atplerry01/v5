using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Registry;

public sealed class RegistryProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.registry";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.registry.created",
        "whyce.trust.identity.registry.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRegistryViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RegistryReadModel
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
