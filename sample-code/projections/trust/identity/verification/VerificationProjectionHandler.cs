using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Verification;

public sealed class VerificationProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.verification";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.verification.created",
        "whyce.trust.identity.verification.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IVerificationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new VerificationReadModel
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
