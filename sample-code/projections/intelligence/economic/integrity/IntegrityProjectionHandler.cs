using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic.Integrity;

public sealed class IntegrityProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic.integrity";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic.integrity.created",
        "whyce.intelligence.economic.integrity.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIntegrityViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IntegrityReadModel
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
