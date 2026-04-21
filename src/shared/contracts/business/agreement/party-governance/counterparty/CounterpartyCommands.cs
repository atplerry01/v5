using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Counterparty;

public sealed record CreateCounterpartyCommand(
    Guid CounterpartyId,
    Guid IdentityReference,
    string Name) : IHasAggregateId
{
    public Guid AggregateId => CounterpartyId;
}

public sealed record SuspendCounterpartyCommand(Guid CounterpartyId) : IHasAggregateId
{
    public Guid AggregateId => CounterpartyId;
}

public sealed record TerminateCounterpartyCommand(Guid CounterpartyId) : IHasAggregateId
{
    public Guid AggregateId => CounterpartyId;
}
