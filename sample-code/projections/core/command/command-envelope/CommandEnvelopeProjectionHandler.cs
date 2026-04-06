using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Command.CommandEnvelope;

public sealed class CommandEnvelopeProjectionHandler
{
    public string ProjectionName => "whyce.core.command.command-envelope";

    public string[] EventTypes =>
    [
        "whyce.core.command.command-envelope.created",
        "whyce.core.command.command-envelope.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICommandEnvelopeViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CommandEnvelopeReadModel
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
