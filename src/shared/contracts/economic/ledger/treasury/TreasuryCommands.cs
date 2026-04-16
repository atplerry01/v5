using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Ledger.Treasury;

public sealed record CreateTreasuryCommand(
    Guid TreasuryId,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => TreasuryId;
}

public sealed record AllocateFundsCommand(
    Guid TreasuryId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => TreasuryId;
}

public sealed record ReleaseFundsCommand(
    Guid TreasuryId,
    decimal Amount) : IHasAggregateId
{
    public Guid AggregateId => TreasuryId;
}
