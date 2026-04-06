using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Handshake;

public sealed class HandshakeProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.handshake";

    public string[] EventTypes =>
    [
        "whyce.business.integration.handshake.created",
        "whyce.business.integration.handshake.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IHandshakeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new HandshakeReadModel
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
