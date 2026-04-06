using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Compliance.ComplianceCase;

public sealed class ComplianceCaseProjectionHandler
{
    public string ProjectionName => "whyce.decision.compliance.compliance-case";

    public string[] EventTypes =>
    [
        "whyce.decision.compliance.compliance-case.created",
        "whyce.decision.compliance.compliance-case.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IComplianceCaseViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ComplianceCaseReadModel
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
