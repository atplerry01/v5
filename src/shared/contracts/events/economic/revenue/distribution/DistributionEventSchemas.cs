namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Distribution;

public sealed record DistributionCreatedEventSchema(
    Guid AggregateId,
    string SpvId,
    decimal TotalAmount);
