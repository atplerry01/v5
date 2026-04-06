using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic.Analysis;

public sealed class AnalysisProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic.analysis";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic.analysis.created",
        "whyce.intelligence.economic.analysis.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAnalysisViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AnalysisReadModel
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
