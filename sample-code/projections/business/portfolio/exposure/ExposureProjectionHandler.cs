using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Portfolio.Exposure;

public sealed class ExposureProjectionHandler
{
    public string ProjectionName => "whyce.business.portfolio.exposure";

    public string[] EventTypes =>
    [
        "whyce.business.portfolio.exposure.created",
        "whyce.business.portfolio.exposure.updated"
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
