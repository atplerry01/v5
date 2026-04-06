using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Core.Financialcontrol.BudgetControl;

public sealed class BudgetControlProjectionHandler
{
    public string ProjectionName => "whyce.core.financialcontrol.budget-control";

    public string[] EventTypes =>
    [
        "whyce.core.financialcontrol.budget-control.created",
        "whyce.core.financialcontrol.budget-control.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBudgetControlViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BudgetControlReadModel
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
