using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Audit.Remediation;

public sealed class RemediationProjectionHandler
{
    public string ProjectionName => "whyce.decision.audit.remediation";

    public string[] EventTypes =>
    [
        "whyce.decision.audit.remediation.created",
        "whyce.decision.audit.remediation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IRemediationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new RemediationReadModel
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
