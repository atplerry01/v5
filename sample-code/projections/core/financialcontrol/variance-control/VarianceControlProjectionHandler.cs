using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Financialcontrol.VarianceControl;

public sealed class VarianceControlProjectionHandler
{
    public string ProjectionName => "whyce.core.financialcontrol.variance-control";

    public string[] EventTypes =>
    [
        "whyce.core.financialcontrol.variance-control.created",
        "whyce.core.financialcontrol.variance-control.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IVarianceControlViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new VarianceControlReadModel
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
