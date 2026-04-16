using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Payout;

public sealed record PayoutShareEntry(
    string ParticipantId,
    decimal Amount,
    decimal Percentage);

public sealed record ExecutePayoutCommand(
    Guid PayoutId,
    Guid DistributionId,
    IReadOnlyList<PayoutShareEntry> Shares) : IHasAggregateId
{
    public Guid AggregateId => PayoutId;
}
