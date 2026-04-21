namespace Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Amendment;

public sealed record AmendmentRequestedEventSchema(
    Guid AggregateId,
    Guid OrderId,
    string Reason,
    DateTimeOffset RequestedAt);

public sealed record AmendmentAcceptedEventSchema(Guid AggregateId, DateTimeOffset AcceptedAt);

public sealed record AmendmentAppliedEventSchema(Guid AggregateId, DateTimeOffset AppliedAt);

public sealed record AmendmentRejectedEventSchema(Guid AggregateId, DateTimeOffset RejectedAt);

public sealed record AmendmentCancelledEventSchema(Guid AggregateId, DateTimeOffset CancelledAt);
