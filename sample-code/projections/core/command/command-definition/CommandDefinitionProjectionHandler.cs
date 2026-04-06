using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Command.CommandDefinition;

public sealed class CommandDefinitionProjectionHandler
{
    public string ProjectionName => "whyce.core.command.command-definition";

    public string[] EventTypes =>
    [
        "whyce.core.command.command-definition.created",
        "whyce.core.command.command-definition.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICommandDefinitionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CommandDefinitionReadModel
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
