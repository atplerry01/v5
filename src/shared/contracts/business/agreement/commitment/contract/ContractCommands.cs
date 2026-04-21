using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.Commitment.Contract;

public sealed record CreateContractCommand(
    Guid ContractId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record AddPartyToContractCommand(
    Guid ContractId,
    Guid PartyId,
    string Role) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record ActivateContractCommand(Guid ContractId) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record SuspendContractCommand(Guid ContractId) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record TerminateContractCommand(Guid ContractId) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}
