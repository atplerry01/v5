using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Constitutional.Chain.Ledger;

public sealed record OpenLedgerCommand(
    Guid LedgerId,
    string LedgerName,
    DateTimeOffset OpenedAt) : IHasAggregateId
{
    public Guid AggregateId => LedgerId;
}

public sealed record SealLedgerCommand(
    Guid LedgerId,
    DateTimeOffset SealedAt) : IHasAggregateId
{
    public Guid AggregateId => LedgerId;
}
