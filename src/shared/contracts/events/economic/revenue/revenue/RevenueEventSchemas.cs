namespace Whycespace.Shared.Contracts.Events.Economic.Revenue.Revenue;

public sealed record RevenueRecordedEventSchema(
    Guid AggregateId,
    string SpvId,
    decimal Amount,
    string Currency,
    string SourceRef);
