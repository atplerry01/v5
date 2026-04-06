using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Observability.Diagnostic;

public sealed class DiagnosticProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.observability.diagnostic";

    public string[] EventTypes =>
    [
        "whyce.intelligence.observability.diagnostic.created",
        "whyce.intelligence.observability.diagnostic.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDiagnosticViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DiagnosticReadModel
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
