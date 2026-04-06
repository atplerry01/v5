using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Billing.Invoice;

public sealed class InvoiceProjectionHandler
{
    public string ProjectionName => "whyce.business.billing.invoice";

    public string[] EventTypes =>
    [
        "whyce.business.billing.invoice.created",
        "whyce.business.billing.invoice.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IInvoiceViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new InvoiceReadModel
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
