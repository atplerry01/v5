using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Contract;

public sealed record ContractShareRule(Guid PartyId, decimal SharePercentage);

public sealed record CreateRevenueContractCommand(
    Guid ContractId,
    IReadOnlyList<ContractShareRule> ShareRules,
    DateTimeOffset TermStart,
    DateTimeOffset TermEnd) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record ActivateRevenueContractCommand(
    Guid ContractId) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}

public sealed record TerminateRevenueContractCommand(
    Guid ContractId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => ContractId;
}