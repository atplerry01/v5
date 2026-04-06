using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Document.Evidence;

public sealed class EvidenceProjectionHandler
{
    public string ProjectionName => "whyce.business.document.evidence";

    public string[] EventTypes =>
    [
        "whyce.business.document.evidence.created",
        "whyce.business.document.evidence.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IEvidenceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new EvidenceReadModel
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
