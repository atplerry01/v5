using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Experiment.ResultAnalysis;

public sealed class ResultAnalysisProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.experiment.result-analysis";

    public string[] EventTypes =>
    [
        "whyce.intelligence.experiment.result-analysis.created",
        "whyce.intelligence.experiment.result-analysis.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IResultAnalysisViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ResultAnalysisReadModel
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
