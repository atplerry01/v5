using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Revenue.Payout;

public sealed record PayoutShareEntry(
    string ParticipantId,
    decimal Amount,
    decimal Percentage);

public sealed record ExecutePayoutCommand(
    Guid PayoutId,
    Guid DistributionId,
    string IdempotencyKey,
    IReadOnlyList<PayoutShareEntry> Shares) : IHasAggregateId
{
    public Guid AggregateId => PayoutId;
}

public sealed record MarkPayoutExecutedCommand(
    Guid PayoutId) : IHasAggregateId
{
    public Guid AggregateId => PayoutId;
}

public sealed record MarkPayoutFailedCommand(
    Guid PayoutId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => PayoutId;
}

public sealed record RequestPayoutCompensationCommand(
    Guid PayoutId,
    string Reason) : IHasAggregateId
{
    public Guid AggregateId => PayoutId;
}

public sealed record MarkPayoutCompensatedCommand(
    Guid PayoutId,
    string CompensatingJournalId) : IHasAggregateId
{
    public Guid AggregateId => PayoutId;
}
