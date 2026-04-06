using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Logistic.Tracking;

public sealed class TrackingProjectionHandler
{
    public string ProjectionName => "whyce.business.logistic.tracking";

    public string[] EventTypes =>
    [
        "whyce.business.logistic.tracking.created",
        "whyce.business.logistic.tracking.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITrackingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TrackingReadModel
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
