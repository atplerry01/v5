using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Transaction.Settlement;

public sealed record InitiateSettlementCommand(
    Guid SettlementId,
    decimal Amount,
    string Currency,
    string SourceReference,
    string Provider) : IHasAggregateId
{
    public Guid AggregateId => SettlementId;
}

public sealed record CompleteSettlementCommand(
    Guid SettlementId,
    string ExternalReferenceId) : IHasAggregateId
{
    public Guid AggregateId => SettlementId;
}

public sealed record FailSettlementCommand(
    Guid SettlementId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => SettlementId;
}
