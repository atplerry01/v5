using Whycespace.Shared.Contracts.Business.Order.OrderCore.Reservation;
using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Reservation;

namespace Whycespace.Projections.Business.Order.OrderCore.Reservation.Reducer;

public static class ReservationProjectionReducer
{
    public static ReservationReadModel Apply(ReservationReadModel state, ReservationHeldEventSchema e) =>
        state with
        {
            ReservationId = e.AggregateId,
            OrderId = e.OrderId,
            LineItemId = e.LineItemId,
            SubjectKind = e.SubjectKind,
            SubjectId = e.SubjectId,
            QuantityValue = e.QuantityValue,
            QuantityUnit = e.QuantityUnit,
            ExpiresAt = e.ExpiresAt,
            Status = "Held",
            HeldAt = e.HeldAt
        };

    public static ReservationReadModel Apply(ReservationReadModel state, ReservationConfirmedEventSchema e) =>
        state with
        {
            ReservationId = e.AggregateId,
            Status = "Confirmed",
            ConfirmedAt = e.ConfirmedAt
        };

    public static ReservationReadModel Apply(ReservationReadModel state, ReservationReleasedEventSchema e) =>
        state with
        {
            ReservationId = e.AggregateId,
            Status = "Released",
            ReleasedAt = e.ReleasedAt
        };

    public static ReservationReadModel Apply(ReservationReadModel state, ReservationExpiredEventSchema e) =>
        state with
        {
            ReservationId = e.AggregateId,
            Status = "Expired",
            ExpiredAt = e.ExpiredAt
        };
}
