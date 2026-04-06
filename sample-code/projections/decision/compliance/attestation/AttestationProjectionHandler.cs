using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Compliance.Attestation;

public sealed class AttestationProjectionHandler
{
    public string ProjectionName => "whyce.decision.compliance.attestation";

    public string[] EventTypes =>
    [
        "whyce.decision.compliance.attestation.created",
        "whyce.decision.compliance.attestation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IAttestationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new AttestationReadModel
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
