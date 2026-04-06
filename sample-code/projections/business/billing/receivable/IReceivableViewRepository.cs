namespace Whycespace.Projections.Business.Billing.Receivable;

public interface IReceivableViewRepository
{
    Task SaveAsync(ReceivableReadModel model, CancellationToken ct = default);
    Task<ReceivableReadModel?> GetAsync(string id, CancellationToken ct = default);
}
