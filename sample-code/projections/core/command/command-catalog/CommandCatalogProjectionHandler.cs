using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Command.CommandCatalog;

public sealed class CommandCatalogProjectionHandler
{
    public string ProjectionName => "whyce.core.command.command-catalog";

    public string[] EventTypes =>
    [
        "whyce.core.command.command-catalog.created",
        "whyce.core.command.command-catalog.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICommandCatalogViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CommandCatalogReadModel
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
