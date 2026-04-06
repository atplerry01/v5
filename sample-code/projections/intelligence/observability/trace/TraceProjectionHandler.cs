using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.Trace;

public sealed class TraceProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.trace";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.trace.created",
        "whyce.intelligence.observability.trace.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITraceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TraceReadModel
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
