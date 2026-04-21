namespace Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Order;

public sealed record OrderCreatedEventSchema(
    Guid AggregateId,
    Guid SourceReferenceId,
    string Description);

public sealed record OrderConfirmedEventSchema(Guid AggregateId);

public sealed record OrderCompletedEventSchema(Guid AggregateId);

public sealed record OrderCancelledEventSchema(
    Guid AggregateId,
    DateTimeOffset CancelledAt);
