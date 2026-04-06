using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Relationship.Affiliation;

public sealed class AffiliationProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.relationship.affiliation";

    public string[] EventTypes =>
    [
        "whyce.intelligence.relationship.affiliation.created",
        "whyce.intelligence.relationship.affiliation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAffiliationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AffiliationReadModel
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
