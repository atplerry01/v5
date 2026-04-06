using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Relationship.Linkage;

public sealed class LinkageProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.relationship.linkage";

    public string[] EventTypes =>
    [
        "whyce.intelligence.relationship.linkage.created",
        "whyce.intelligence.relationship.linkage.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILinkageViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LinkageReadModel
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
