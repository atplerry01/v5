namespace Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.LineItem;

public sealed record LineItemCreatedEventSchema(
    Guid AggregateId,
    Guid OrderId,
    int SubjectKind,
    Guid SubjectId,
    decimal QuantityValue,
    string QuantityUnit);

public sealed record LineItemUpdatedEventSchema(
    Guid AggregateId,
    decimal QuantityValue,
    string QuantityUnit);

public sealed record LineItemCancelledEventSchema(Guid AggregateId);
