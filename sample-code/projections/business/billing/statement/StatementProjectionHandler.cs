using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Billing.Statement;

public sealed class StatementProjectionHandler
{
    public string ProjectionName => "whyce.business.billing.statement";

    public string[] EventTypes =>
    [
        "whyce.business.billing.statement.created",
        "whyce.business.billing.statement.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IStatementViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new StatementReadModel
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
