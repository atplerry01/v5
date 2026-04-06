using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Access.Grant;

public sealed class GrantProjectionHandler
{
    public string ProjectionName => "whyce.trust.access.grant";

    public string[] EventTypes =>
    [
        "whyce.trust.access.grant.created",
        "whyce.trust.access.grant.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IGrantViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new GrantReadModel
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
