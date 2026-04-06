namespace Whycespace.Projections.Business.Inventory.Writeoff;

public interface IWriteoffViewRepository
{
    Task SaveAsync(WriteoffReadModel model, CancellationToken ct = default);
    Task<WriteoffReadModel?> GetAsync(string id, CancellationToken ct = default);
}
