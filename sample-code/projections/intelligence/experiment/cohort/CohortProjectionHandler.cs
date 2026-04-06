using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Experiment.Cohort;

public sealed class CohortProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.experiment.cohort";

    public string[] EventTypes =>
    [
        "whyce.intelligence.experiment.cohort.created",
        "whyce.intelligence.experiment.cohort.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICohortViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CohortReadModel
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
