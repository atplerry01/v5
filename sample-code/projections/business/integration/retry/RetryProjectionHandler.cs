using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Retry;

public sealed class RetryProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.retry";

    public string[] EventTypes =>
    [
        "whyce.business.integration.retry.created",
        "whyce.business.integration.retry.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRetryViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RetryReadModel
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
