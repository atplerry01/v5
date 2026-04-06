using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Trust;

public sealed class TrustProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.trust";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.trust.created",
        "whyce.trust.identity.trust.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITrustViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TrustReadModel
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
