using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Capacity.Constraint;

public sealed class ConstraintProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.capacity.constraint";

    public string[] EventTypes =>
    [
        "whyce.intelligence.capacity.constraint.created",
        "whyce.intelligence.capacity.constraint.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IConstraintViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ConstraintReadModel
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
