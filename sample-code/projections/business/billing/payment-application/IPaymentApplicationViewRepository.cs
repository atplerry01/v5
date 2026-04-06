namespace Whycespace.Projections.Business.Billing.PaymentApplication;

public interface IPaymentApplicationViewRepository
{
    Task SaveAsync(PaymentApplicationReadModel model, CancellationToken ct = default);
    Task<PaymentApplicationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
