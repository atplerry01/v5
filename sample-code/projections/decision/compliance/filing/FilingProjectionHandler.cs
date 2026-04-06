using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Compliance.Filing;

public sealed class FilingProjectionHandler
{
    public string ProjectionName => "whyce.decision.compliance.filing";

    public string[] EventTypes =>
    [
        "whyce.decision.compliance.filing.created",
        "whyce.decision.compliance.filing.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IFilingViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new FilingReadModel
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
