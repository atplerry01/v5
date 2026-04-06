using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Logistic.Dispatch;

public sealed class DispatchProjectionHandler
{
    public string ProjectionName => "whyce.business.logistic.dispatch";

    public string[] EventTypes =>
    [
        "whyce.business.logistic.dispatch.created",
        "whyce.business.logistic.dispatch.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IDispatchViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new DispatchReadModel
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
