using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.CommandBridge;

public sealed class CommandBridgeProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.command-bridge";

    public string[] EventTypes =>
    [
        "whyce.business.integration.command-bridge.created",
        "whyce.business.integration.command-bridge.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICommandBridgeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CommandBridgeReadModel
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
