namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;

public sealed record PayoutExecutedEventSchema(
    Guid AggregateId,
    Guid DistributionId);
