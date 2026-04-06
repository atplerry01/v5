using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Command.CommandRouting;

public sealed class CommandRoutingProjectionHandler
{
    public string ProjectionName => "whyce.core.command.command-routing";

    public string[] EventTypes =>
    [
        "whyce.core.command.command-routing.created",
        "whyce.core.command.command-routing.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICommandRoutingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CommandRoutingReadModel
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
