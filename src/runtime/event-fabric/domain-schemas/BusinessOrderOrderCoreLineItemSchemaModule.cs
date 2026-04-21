using Whycespace.Shared.Contracts.Events.Business.Order.OrderCore.LineItem;
using DomainEvents = Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the business-system/order/order-core/line-item domain.
/// </summary>
public sealed class BusinessOrderOrderCoreLineItemSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "LineItemCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LineItemCreatedEvent),
            typeof(LineItemCreatedEventSchema));

        sink.RegisterSchema(
            "LineItemUpdatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.LineItemUpdatedEvent),
            typeof(LineItemUpdatedEventSchema));

        sink.RegisterSchema(
            "LineItemCancelledEvent",
            EventVersion.Default,
            typeof(DomainEvents.LineItemCancelledEvent),
            typeof(LineItemCancelledEventSchema));

        sink.RegisterPayloadMapper("LineItemCreatedEvent", e =>
        {
            var evt = (DomainEvents.LineItemCreatedEvent)e;
            return new LineItemCreatedEventSchema(
                evt.LineItemId.Value,
                evt.Order.Value,
                (int)evt.Subject.Kind,
                evt.Subject.SubjectId.Value,
                evt.Quantity.Value,
                evt.Quantity.Unit);
        });
        sink.RegisterPayloadMapper("LineItemUpdatedEvent", e =>
        {
            var evt = (DomainEvents.LineItemUpdatedEvent)e;
            return new LineItemUpdatedEventSchema(
                evt.LineItemId.Value,
                evt.Quantity.Value,
                evt.Quantity.Unit);
        });
        sink.RegisterPayloadMapper("LineItemCancelledEvent", e =>
        {
            var evt = (DomainEvents.LineItemCancelledEvent)e;
            return new LineItemCancelledEventSchema(evt.LineItemId.Value);
        });
    }
}
