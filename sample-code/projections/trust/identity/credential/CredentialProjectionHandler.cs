using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Identity.Credential;

public sealed class CredentialProjectionHandler
{
    public string ProjectionName => "whyce.trust.identity.credential";

    public string[] EventTypes =>
    [
        "whyce.trust.identity.credential.created",
        "whyce.trust.identity.credential.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICredentialViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CredentialReadModel
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
