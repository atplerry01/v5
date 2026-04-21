using Whycespace.Shared.Contracts.Events.Business.Order.OrderChange.FulfillmentInstruction;
using DomainEvents = Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class BusinessOrderOrderChangeFulfillmentInstructionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "FulfillmentInstructionCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.FulfillmentInstructionCreatedEvent),
            typeof(FulfillmentInstructionCreatedEventSchema));

        sink.RegisterSchema(
            "FulfillmentInstructionIssuedEvent",
            EventVersion.Default,
            typeof(DomainEvents.FulfillmentInstructionIssuedEvent),
            typeof(FulfillmentInstructionIssuedEventSchema));

        sink.RegisterSchema(
            "FulfillmentInstructionCompletedEvent",
            EventVersion.Default,
            typeof(DomainEvents.FulfillmentInstructionCompletedEvent),
            typeof(FulfillmentInstructionCompletedEventSchema));

        sink.RegisterSchema(
            "FulfillmentInstructionRevokedEvent",
            EventVersion.Default,
            typeof(DomainEvents.FulfillmentInstructionRevokedEvent),
            typeof(FulfillmentInstructionRevokedEventSchema));

        sink.RegisterPayloadMapper("FulfillmentInstructionCreatedEvent", e =>
        {
            var evt = (DomainEvents.FulfillmentInstructionCreatedEvent)e;
            return new FulfillmentInstructionCreatedEventSchema(
                evt.FulfillmentInstructionId.Value,
                evt.Order.Value,
                evt.LineItem?.Value,
                evt.Directive.Value);
        });
        sink.RegisterPayloadMapper("FulfillmentInstructionIssuedEvent", e =>
        {
            var evt = (DomainEvents.FulfillmentInstructionIssuedEvent)e;
            return new FulfillmentInstructionIssuedEventSchema(evt.FulfillmentInstructionId.Value, evt.IssuedAt);
        });
        sink.RegisterPayloadMapper("FulfillmentInstructionCompletedEvent", e =>
        {
            var evt = (DomainEvents.FulfillmentInstructionCompletedEvent)e;
            return new FulfillmentInstructionCompletedEventSchema(evt.FulfillmentInstructionId.Value, evt.CompletedAt);
        });
        sink.RegisterPayloadMapper("FulfillmentInstructionRevokedEvent", e =>
        {
            var evt = (DomainEvents.FulfillmentInstructionRevokedEvent)e;
            return new FulfillmentInstructionRevokedEventSchema(evt.FulfillmentInstructionId.Value, evt.RevokedAt);
        });
    }
}
