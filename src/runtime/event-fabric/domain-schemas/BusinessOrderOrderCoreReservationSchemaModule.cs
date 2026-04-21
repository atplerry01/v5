using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Reservation;
using DomainEvents = Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/order/order-core/reservation domain.
/// </summary>
public sealed class BusinessOrderOrderCoreReservationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "ReservationHeldEvent",
            EventVersion.Default,
            typeof(DomainEvents.ReservationHeldEvent),
            typeof(ReservationHeldEventSchema));

        sink.RegisterSchema(
            "ReservationConfirmedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ReservationConfirmedEvent),
            typeof(ReservationConfirmedEventSchema));

        sink.RegisterSchema(
            "ReservationReleasedEvent",
            EventVersion.Default,
            typeof(DomainEvents.ReservationReleasedEvent),
            typeof(ReservationReleasedEventSchema));

        sink.RegisterSchema(
            "ReservationExpiredEvent",
            EventVersion.Default,
            typeof(DomainEvents.ReservationExpiredEvent),
            typeof(ReservationExpiredEventSchema));

        sink.RegisterPayloadMapper("ReservationHeldEvent", e =>
        {
            var evt = (DomainEvents.ReservationHeldEvent)e;
            return new ReservationHeldEventSchema(
                evt.ReservationId.Value,
                evt.Order.Value,
                evt.LineItem?.Value,
                (int)evt.Subject.Kind,
                evt.Subject.SubjectId.Value,
                evt.Quantity.Value,
                evt.Quantity.Unit,
                evt.Expiry.ExpiresAt,
                evt.HeldAt);
        });
        sink.RegisterPayloadMapper("ReservationConfirmedEvent", e =>
        {
            var evt = (DomainEvents.ReservationConfirmedEvent)e;
            return new ReservationConfirmedEventSchema(evt.ReservationId.Value, evt.ConfirmedAt);
        });
        sink.RegisterPayloadMapper("ReservationReleasedEvent", e =>
        {
            var evt = (DomainEvents.ReservationReleasedEvent)e;
            return new ReservationReleasedEventSchema(evt.ReservationId.Value, evt.ReleasedAt);
        });
        sink.RegisterPayloadMapper("ReservationExpiredEvent", e =>
        {
            var evt = (DomainEvents.ReservationExpiredEvent)e;
            return new ReservationExpiredEventSchema(evt.ReservationId.Value, evt.ExpiredAt);
        });
    }
}
