namespace Whycespace.Projections.Business.Inventory.Transfer;

public interface ITransferViewRepository
{
    Task SaveAsync(TransferReadModel model, CancellationToken ct = default);
    Task<TransferReadModel?> GetAsync(string id, CancellationToken ct = default);
}
