using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Structural.Humancapital.Operator;

public sealed class OperatorProjectionHandler
{
    public string ProjectionName => "whyce.structural.humancapital.operator";

    public string[] EventTypes =>
    [
        "whyce.structural.humancapital.operator.created",
        "whyce.structural.humancapital.operator.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IOperatorViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new OperatorReadModel
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
