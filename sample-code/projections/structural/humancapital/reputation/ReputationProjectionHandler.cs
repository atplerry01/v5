using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Reputation;

public sealed class ReputationProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.reputation";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.reputation.created",
        "whyce.structural.humancapital.reputation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReputationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReputationReadModel
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
