using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Economic._shared;

public sealed class _sharedProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.economic._shared";

    public string[] EventTypes =>
    [
        "whyce.intelligence.economic._shared.created",
        "whyce.intelligence.economic._shared.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, I_sharedViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new _sharedReadModel
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
