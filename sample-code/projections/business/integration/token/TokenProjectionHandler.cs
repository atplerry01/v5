using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Token;

public sealed class TokenProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.token";

    public string[] EventTypes =>
    [
        "whyce.business.integration.token.created",
        "whyce.business.integration.token.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITokenViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TokenReadModel
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
