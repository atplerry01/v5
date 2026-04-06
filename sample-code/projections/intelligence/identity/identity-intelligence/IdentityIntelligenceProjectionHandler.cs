using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Identity.IdentityIntelligence;

public sealed class IdentityIntelligenceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.identity.identity-intelligence";

    public string[] EventTypes =>
    [
        "whyce.intelligence.identity.identity-intelligence.created",
        "whyce.intelligence.identity.identity-intelligence.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIdentityIntelligenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IdentityIntelligenceReadModel
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
