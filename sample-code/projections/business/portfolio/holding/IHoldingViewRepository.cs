namespace Whycespace.Projections.Business.Portfolio.Holding;

public interface IHoldingViewRepository
{
    Task SaveAsync(HoldingReadModel model, CancellationToken ct = default);
    Task<HoldingReadModel?> GetAsync(string id, CancellationToken ct = default);
}
