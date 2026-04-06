using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Consent;

public sealed class ConsentProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.consent";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.consent.created",
        "whyce.trust.identity.consent.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IConsentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ConsentReadModel
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
