using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Experiment.Variant;

public sealed class VariantProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.experiment.variant";

    public string[] EventTypes =>
    [
        "whyce.intelligence.experiment.variant.created",
        "whyce.intelligence.experiment.variant.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IVariantViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new VariantReadModel
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
