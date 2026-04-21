using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;

public sealed record HoldReservationCommand(
    Guid ReservationId,
    Guid OrderId,
    Guid? LineItemId,
    int SubjectKind,
    Guid SubjectId,
    decimal QuantityValue,
    string QuantityUnit,
    DateTimeOffset ExpiresAt,
    DateTimeOffset HeldAt) : IHasAggregateId
{
    public Guid AggregateId => ReservationId;
}

public sealed record ConfirmReservationCommand(
    Guid ReservationId,
    DateTimeOffset ConfirmedAt) : IHasAggregateId
{
    public Guid AggregateId => ReservationId;
}

public sealed record ReleaseReservationCommand(
    Guid ReservationId,
    DateTimeOffset ReleasedAt) : IHasAggregateId
{
    public Guid AggregateId => ReservationId;
}

public sealed record ExpireReservationCommand(
    Guid ReservationId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => ReservationId;
}
