using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Simulation.Model;

public sealed class ModelProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.simulation.model";

    public string[] EventTypes =>
    [
        "whyce.intelligence.simulation.model.created",
        "whyce.intelligence.simulation.model.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IModelViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ModelReadModel
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
