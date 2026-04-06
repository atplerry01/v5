using Whycespace.Projections.Economic;

namespace Whycespace.Projections.Economic.Transaction.Transaction;

public interface ITransactionViewRepository
{
    Task SaveAsync(TransactionHistoryReadModel model, CancellationToken ct = default);
    Task<TransactionHistoryReadModel?> GetAsync(string id, CancellationToken ct = default);
}
