namespace Whycespace.Projections.Economic.Revenue.Revenue;

public interface IRevenueViewRepository
{
    Task SaveAsync(RevenueReadModel model, CancellationToken ct = default);
    Task<RevenueReadModel?> GetAsync(string id, CancellationToken ct = default);
}
