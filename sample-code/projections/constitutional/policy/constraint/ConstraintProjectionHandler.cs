using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Constraint;

public sealed class ConstraintProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.constraint";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.constraint.created",
        "whyce.constitutional.policy.constraint.updated"
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
