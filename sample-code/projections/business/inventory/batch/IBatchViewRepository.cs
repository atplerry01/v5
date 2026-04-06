namespace Whycespace.Projections.Business.Inventory.Batch;

public interface IBatchViewRepository
{
    Task SaveAsync(BatchReadModel model, CancellationToken ct = default);
    Task<BatchReadModel?> GetAsync(string id, CancellationToken ct = default);
}
