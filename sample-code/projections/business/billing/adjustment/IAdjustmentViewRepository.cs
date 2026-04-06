namespace Whycespace.Projections.Business.Billing.Adjustment;

public interface IAdjustmentViewRepository
{
    Task SaveAsync(AdjustmentReadModel model, CancellationToken ct = default);
    Task<AdjustmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
