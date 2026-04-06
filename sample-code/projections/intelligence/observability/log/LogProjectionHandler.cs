using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.Log;

public sealed class LogProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.log";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.log.created",
        "whyce.intelligence.observability.log.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ILogViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new LogReadModel
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
