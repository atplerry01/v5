using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Experiment.Hypothesis;

public sealed class HypothesisProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.experiment.hypothesis";

    public string[] EventTypes =>
    [
        "whyce.intelligence.experiment.hypothesis.created",
        "whyce.intelligence.experiment.hypothesis.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IHypothesisViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new HypothesisReadModel
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
