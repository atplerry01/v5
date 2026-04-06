using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Operational.Global.IncidentResponse;

public sealed class IncidentResponseProjectionHandler
{
    public string ProjectionName => "whyce.operational.global.incident-response";

    public string[] EventTypes =>
    [
        "whyce.operational.global.incident-response.created",
        "whyce.operational.global.incident-response.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IIncidentResponseViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new IncidentResponseReadModel
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
