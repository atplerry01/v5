using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Planning.Target;

public sealed class TargetProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.planning.target";

    public string[] EventTypes =>
    [
        "whyce.intelligence.planning.target.created",
        "whyce.intelligence.planning.target.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ITargetViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new TargetReadModel
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
