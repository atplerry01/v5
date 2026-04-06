using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Incentive;

public sealed class IncentiveProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.incentive";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.incentive.created",
        "whyce.structural.humancapital.incentive.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIncentiveViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IncentiveReadModel
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
