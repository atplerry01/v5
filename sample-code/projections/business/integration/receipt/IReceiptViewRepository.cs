namespace Whycespace.Projections.Business.Integration.Receipt;

public interface IReceiptViewRepository
{
    Task SaveAsync(ReceiptReadModel model, CancellationToken ct = default);
    Task<ReceiptReadModel?> GetAsync(string id, CancellationToken ct = default);
}
