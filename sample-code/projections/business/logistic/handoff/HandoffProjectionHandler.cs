using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Logistic.Handoff;

public sealed class HandoffProjectionHandler
{
    public string ProjectionName => "whyce.business.logistic.handoff";

    public string[] EventTypes =>
    [
        "whyce.business.logistic.handoff.created",
        "whyce.business.logistic.handoff.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IHandoffViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new HandoffReadModel
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
