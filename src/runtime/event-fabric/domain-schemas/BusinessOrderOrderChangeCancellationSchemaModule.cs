using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.Cancellation;
using DomainEvents = Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/order/order-change/cancellation domain.
/// </summary>
public sealed class BusinessOrderOrderChangeCancellationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "CancellationRequestedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CancellationRequestedEvent),
            typeof(CancellationRequestedEventSchema));

        sink.RegisterSchema(
            "CancellationConfirmedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CancellationConfirmedEvent),
            typeof(CancellationConfirmedEventSchema));

        sink.RegisterSchema(
            "CancellationRejectedEvent",
            EventVersion.Default,
            typeof(DomainEvents.CancellationRejectedEvent),
            typeof(CancellationRejectedEventSchema));

        sink.RegisterPayloadMapper("CancellationRequestedEvent", e =>
        {
            var evt = (DomainEvents.CancellationRequestedEvent)e;
            return new CancellationRequestedEventSchema(
                evt.CancellationId.Value,
                evt.Order.Value,
                evt.Reason.Value,
                evt.RequestedAt);
        });
        sink.RegisterPayloadMapper("CancellationConfirmedEvent", e =>
        {
            var evt = (DomainEvents.CancellationConfirmedEvent)e;
            return new CancellationConfirmedEventSchema(evt.CancellationId.Value, evt.ConfirmedAt);
        });
        sink.RegisterPayloadMapper("CancellationRejectedEvent", e =>
        {
            var evt = (DomainEvents.CancellationRejectedEvent)e;
            return new CancellationRejectedEventSchema(evt.CancellationId.Value, evt.RejectedAt);
        });
    }
}
