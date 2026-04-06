namespace Whycespace.Projections.Business.Marketplace.Catalog;

public interface ICatalogViewRepository
{
    Task SaveAsync(CatalogReadModel model, CancellationToken ct = default);
    Task<CatalogReadModel?> GetAsync(string id, CancellationToken ct = default);
}
