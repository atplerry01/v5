using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Ledger.Ledger;

public sealed record OpenLedgerCommand(
    Guid LedgerId,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => LedgerId;
}
