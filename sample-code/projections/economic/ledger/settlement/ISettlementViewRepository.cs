namespace Whycespace.Projections.Economic.Ledger.Settlement;

public interface ISettlementViewRepository
{
    Task SaveAsync(SettlementReadModel model, CancellationToken ct = default);
    Task<SettlementReadModel?> GetAsync(string id, CancellationToken ct = default);
}
