namespace Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Cancellation;

public sealed record CancellationRequestedEventSchema(
    Guid AggregateId,
    Guid OrderId,
    string Reason,
    DateTimeOffset RequestedAt);

public sealed record CancellationConfirmedEventSchema(Guid AggregateId, DateTimeOffset ConfirmedAt);

public sealed record CancellationRejectedEventSchema(Guid AggregateId, DateTimeOffset RejectedAt);
