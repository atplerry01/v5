namespace Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Reservation;

public sealed record ReservationHeldEventSchema(
    Guid AggregateId,
    Guid OrderId,
    Guid? LineItemId,
    int SubjectKind,
    Guid SubjectId,
    decimal QuantityValue,
    string QuantityUnit,
    DateTimeOffset ExpiresAt,
    DateTimeOffset HeldAt);

public sealed record ReservationConfirmedEventSchema(
    Guid AggregateId,
    DateTimeOffset ConfirmedAt);

public sealed record ReservationReleasedEventSchema(
    Guid AggregateId,
    DateTimeOffset ReleasedAt);

public sealed record ReservationExpiredEventSchema(
    Guid AggregateId,
    DateTimeOffset ExpiredAt);
