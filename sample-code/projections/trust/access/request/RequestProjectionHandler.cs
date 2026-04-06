using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Trust.Access.Request;

public sealed class RequestProjectionHandler
{
    public string ProjectionName => "whyce.trust.access.request";

    public string[] EventTypes =>
    [
        "whyce.trust.access.request.created",
        "whyce.trust.access.request.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRequestViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RequestReadModel
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
