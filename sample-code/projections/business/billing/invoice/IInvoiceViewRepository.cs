namespace Whycespace.Projections.Business.Billing.Invoice;

public interface IInvoiceViewRepository
{
    Task SaveAsync(InvoiceReadModel model, CancellationToken ct = default);
    Task<InvoiceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
