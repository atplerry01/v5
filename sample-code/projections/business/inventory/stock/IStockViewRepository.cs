namespace Whycespace.Projections.Business.Inventory.Stock;

public interface IStockViewRepository
{
    Task SaveAsync(StockReadModel model, CancellationToken ct = default);
    Task<StockReadModel?> GetAsync(string id, CancellationToken ct = default);
}
