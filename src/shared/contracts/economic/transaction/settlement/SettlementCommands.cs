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

/// <summary>
/// Phase 6 T6.4 — provider callback path. Dispatched when the external
/// settlement provider confirms that funds have reached an irrevocable
/// state. Transitions SettlementFinalityRecord on the workflow state
/// from Pending → Confirmed. Functionally equivalent to
/// <see cref="CompleteSettlementCommand"/>: both drive the aggregate to
/// <c>SettlementStatus.Completed</c>; this alias exists so the provider
/// callback contract is explicit in controllers / API surface and
/// audited separately from user-originated completion calls.
/// </summary>
public sealed record ConfirmSettlementFinalityCommand(
    Guid SettlementId,
    string ExternalReferenceId) : IHasAggregateId
{
    public Guid AggregateId => SettlementId;
}
