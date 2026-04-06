namespace Whycespace.Projections.Business.Inventory.Sku;

public interface ISkuViewRepository
{
    Task SaveAsync(SkuReadModel model, CancellationToken ct = default);
    Task<SkuReadModel?> GetAsync(string id, CancellationToken ct = default);
}
