using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Constitutional.Policy.Violation;

public sealed class ViolationProjectionHandler
{
    public string ProjectionName => "whyce.constitutional.policy.violation";

    public string[] EventTypes =>
    [
        "whyce.constitutional.policy.violation.created",
        "whyce.constitutional.policy.violation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IViolationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ViolationReadModel
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
