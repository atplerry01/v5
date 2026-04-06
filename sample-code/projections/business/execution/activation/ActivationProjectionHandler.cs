using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Execution.Activation;

public sealed class ActivationProjectionHandler
{
    public string ProjectionName => "whyce.business.execution.activation";

    public string[] EventTypes =>
    [
        "whyce.business.execution.activation.created",
        "whyce.business.execution.activation.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IActivationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ActivationReadModel
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
