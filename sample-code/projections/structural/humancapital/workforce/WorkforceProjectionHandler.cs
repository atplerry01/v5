using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Workforce;

public sealed class WorkforceProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.workforce";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.workforce.created",
        "whyce.structural.humancapital.workforce.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IWorkforceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new WorkforceReadModel
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
