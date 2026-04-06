using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Risk.Exposure;

public sealed class ExposureProjectionHandler
{
    public string ProjectionName => "whyce.decision.risk.exposure";

    public string[] EventTypes =>
    [
        "whyce.decision.risk.exposure.created",
        "whyce.decision.risk.exposure.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IExposureViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ExposureReadModel
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
