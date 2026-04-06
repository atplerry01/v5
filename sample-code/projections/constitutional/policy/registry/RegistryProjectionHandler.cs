using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Registry;

public sealed class RegistryProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.registry";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.registry.created",
        "whyce.constitutional.policy.registry.updated"
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
