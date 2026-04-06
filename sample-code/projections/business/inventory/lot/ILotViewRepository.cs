namespace Whycespace.Projections.Business.Inventory.Lot;

public interface ILotViewRepository
{
    Task SaveAsync(LotReadModel model, CancellationToken ct = default);
    Task<LotReadModel?> GetAsync(string id, CancellationToken ct = default);
}
