using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Compliance.Regulation;

public sealed class RegulationProjectionHandler
{
    public string ProjectionName => "whyce.decision.compliance.regulation";

    public string[] EventTypes =>
    [
        "whyce.decision.compliance.regulation.created",
        "whyce.decision.compliance.regulation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRegulationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RegulationReadModel
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
