using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Callback;

public sealed class CallbackProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.callback";

    public string[] EventTypes =>
    [
        "whyce.business.integration.callback.created",
        "whyce.business.integration.callback.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ICallbackViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new CallbackReadModel
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
