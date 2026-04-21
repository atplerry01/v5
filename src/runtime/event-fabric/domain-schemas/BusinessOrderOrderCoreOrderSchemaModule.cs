using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.Order;
using DomainEvents = Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/order/order-core/order domain.
/// </summary>
public sealed class BusinessOrderOrderCoreOrderSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "OrderCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OrderCreatedEvent),
            typeof(OrderCreatedEventSchema));

        sink.RegisterSchema(
            "OrderConfirmedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OrderConfirmedEvent),
            typeof(OrderConfirmedEventSchema));

        sink.RegisterSchema(
            "OrderCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.OrderCompletedEvent),
            typeof(OrderCompletedEventSchema));

        sink.RegisterSchema(
            "OrderCancelledEvent",
            EventVersion.Default,
            typeof(DomainEvents.OrderCancelledEvent),
            typeof(OrderCancelledEventSchema));

        sink.RegisterPayloadMapper("OrderCreatedEvent", e =>
        {
            var evt = (DomainEvents.OrderCreatedEvent)e;
            return new OrderCreatedEventSchema(
                evt.OrderId.Value,
                evt.SourceReference.Value,
                evt.Description.Value);
        });
        sink.RegisterPayloadMapper("OrderConfirmedEvent", e =>
        {
            var evt = (DomainEvents.OrderConfirmedEvent)e;
            return new OrderConfirmedEventSchema(evt.OrderId.Value);
        });
        sink.RegisterPayloadMapper("OrderCompletedEvent", e =>
        {
            var evt = (DomainEvents.OrderCompletedEvent)e;
            return new OrderCompletedEventSchema(evt.OrderId.Value);
        });
        sink.RegisterPayloadMapper("OrderCancelledEvent", e =>
        {
            var evt = (DomainEvents.OrderCancelledEvent)e;
            return new OrderCancelledEventSchema(evt.OrderId.Value, evt.CancelledAt);
        });
    }
}
