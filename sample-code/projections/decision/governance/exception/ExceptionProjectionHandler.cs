using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Decision.Governance.Exception;

public sealed class ExceptionProjectionHandler
{
    public string ProjectionName => "whyce.decision.governance.exception";

    public string[] EventTypes =>
    [
        "whyce.decision.governance.exception.created",
        "whyce.decision.governance.exception.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IExceptionViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ExceptionReadModel
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
