using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Version;

public sealed class VersionProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.version";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.version.created",
        "whyce.constitutional.policy.version.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IVersionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new VersionReadModel
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
