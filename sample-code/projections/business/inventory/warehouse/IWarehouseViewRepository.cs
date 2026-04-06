namespace Whycespace.Projections.Business.Inventory.Warehouse;

public interface IWarehouseViewRepository
{
    Task SaveAsync(WarehouseReadModel model, CancellationToken ct = default);
    Task<WarehouseReadModel?> GetAsync(string id, CancellationToken ct = default);
}
