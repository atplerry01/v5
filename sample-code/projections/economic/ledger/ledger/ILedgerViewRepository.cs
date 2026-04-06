using Whycespace.Projections.Economic;

namespace Whycespace.Projections.Economic.Ledger.Ledger;

public interface ILedgerViewRepository
{
    Task SaveAsync(LedgerPolicyLinkReadModel model, CancellationToken ct = default);
    Task<LedgerPolicyLinkReadModel?> GetAsync(string id, CancellationToken ct = default);
}
