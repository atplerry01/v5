using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Secret;

public sealed class SecretProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.secret";

    public string[] EventTypes =>
    [
        "whyce.business.integration.secret.created",
        "whyce.business.integration.secret.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISecretViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SecretReadModel
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
