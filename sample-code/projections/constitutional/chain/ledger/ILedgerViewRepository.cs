namespace Whycespace.Projections.Constitutional.Chain.Ledger;

public interface ILedgerViewRepository
{
    Task SaveAsync(LedgerReadModel model, CancellationToken ct = default);
    Task<LedgerReadModel?> GetAsync(string id, CancellationToken ct = default);
}
