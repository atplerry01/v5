using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Business.Billing.PaymentApplication;

public sealed class PaymentApplicationProjectionHandler
{
    public string ProjectionName => "whyce.business.billing.payment-application";

    public string[] EventTypes =>
    [
        "whyce.business.billing.payment-application.created",
        "whyce.business.billing.payment-application.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IPaymentApplicationViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new PaymentApplicationReadModel
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
