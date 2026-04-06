using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Compliance.Jurisdiction;

public sealed class JurisdictionProjectionHandler
{
    public string ProjectionName => "whyce.decision.compliance.jurisdiction";

    public string[] EventTypes =>
    [
        "whyce.decision.compliance.jurisdiction.created",
        "whyce.decision.compliance.jurisdiction.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IJurisdictionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new JurisdictionReadModel
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
