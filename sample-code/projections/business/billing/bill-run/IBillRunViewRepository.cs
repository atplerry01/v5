namespace Whycespace.Projections.Business.Billing.BillRun;

public interface IBillRunViewRepository
{
    Task SaveAsync(BillRunReadModel model, CancellationToken ct = default);
    Task<BillRunReadModel?> GetAsync(string id, CancellationToken ct = default);
}
