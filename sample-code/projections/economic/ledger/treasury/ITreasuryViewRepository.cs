namespace Whycespace.Projections.Economic.Ledger.Treasury;

public interface ITreasuryViewRepository
{
    Task SaveAsync(TreasuryReadModel model, CancellationToken ct = default);
    Task<TreasuryReadModel?> GetAsync(string id, CancellationToken ct = default);
}
