using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Ledger.Obligation;

public sealed record CreateObligationCommand(
    Guid ObligationId,
    Guid CounterpartyId,
    string Type,
    decimal Amount,
    string Currency) : IHasAggregateId
{
    public Guid AggregateId => ObligationId;
}

public sealed record FulfilObligationCommand(
    Guid ObligationId,
    Guid SettlementId) : IHasAggregateId
{
    public Guid AggregateId => ObligationId;
}

public sealed record CancelObligationCommand(
    Guid ObligationId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => ObligationId;
}
