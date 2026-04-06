namespace Whycespace.Projections.Business.Inventory.Count;

public interface ICountViewRepository
{
    Task SaveAsync(CountReadModel model, CancellationToken ct = default);
    Task<CountReadModel?> GetAsync(string id, CancellationToken ct = default);
}
