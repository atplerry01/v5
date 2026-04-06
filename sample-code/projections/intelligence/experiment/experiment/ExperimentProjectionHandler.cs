using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Experiment.Experiment;

public sealed class ExperimentProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.experiment.experiment";

    public string[] EventTypes =>
    [
        "whyce.intelligence.experiment.experiment.created",
        "whyce.intelligence.experiment.experiment.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IExperimentViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ExperimentReadModel
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
