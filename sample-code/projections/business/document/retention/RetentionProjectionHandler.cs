using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Document.Retention;

public sealed class RetentionProjectionHandler
{
    public string ProjectionName => "whyce.business.document.retention";

    public string[] EventTypes =>
    [
        "whyce.business.document.retention.created",
        "whyce.business.document.retention.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRetentionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RetentionReadModel
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
