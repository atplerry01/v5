using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Financialcontrol.SpendControl;

public sealed class SpendControlProjectionHandler
{
    public string ProjectionName => "whyce.core.financialcontrol.spend-control";

    public string[] EventTypes =>
    [
        "whyce.core.financialcontrol.spend-control.created",
        "whyce.core.financialcontrol.spend-control.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, ISpendControlViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new SpendControlReadModel
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
