namespace Whycespace.Projections.Business.Inventory.Replenishment;

public interface IReplenishmentViewRepository
{
    Task SaveAsync(ReplenishmentReadModel model, CancellationToken ct = default);
    Task<ReplenishmentReadModel?> GetAsync(string id, CancellationToken ct = default);
}
