namespace Whycespace.Projections.Business.Inventory.Item;

public interface IItemViewRepository
{
    Task SaveAsync(ItemReadModel model, CancellationToken ct = default);
    Task<ItemReadModel?> GetAsync(string id, CancellationToken ct = default);
}
