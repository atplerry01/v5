using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Billing.BillRun;

public sealed class BillRunProjectionHandler
{
    public string ProjectionName => "whyce.business.billing.bill-run";

    public string[] EventTypes =>
    [
        "whyce.business.billing.bill-run.created",
        "whyce.business.billing.bill-run.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IBillRunViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new BillRunReadModel
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
