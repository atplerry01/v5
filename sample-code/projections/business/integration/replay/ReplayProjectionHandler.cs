using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Integration.Replay;

public sealed class ReplayProjectionHandler
{
    public string ProjectionName => "whyce.business.integration.replay";

    public string[] EventTypes =>
    [
        "whyce.business.integration.replay.created",
        "whyce.business.integration.replay.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IReplayViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ReplayReadModel
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
