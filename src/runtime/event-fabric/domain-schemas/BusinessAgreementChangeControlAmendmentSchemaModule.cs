using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Amendment;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class BusinessAgreementChangeControlAmendmentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "AmendmentCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentCreatedEvent),
            typeof(AmendmentCreatedEventSchema));

        sink.RegisterSchema(
            "AmendmentAppliedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentAppliedEvent),
            typeof(AmendmentAppliedEventSchema));

        sink.RegisterSchema(
            "AmendmentRevertedEvent",
            EventVersion.Default,
            typeof(DomainEvents.AmendmentRevertedEvent),
            typeof(AmendmentRevertedEventSchema));

        sink.RegisterPayloadMapper("AmendmentCreatedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentCreatedEvent)e;
            return new AmendmentCreatedEventSchema(evt.AmendmentId.Value, evt.TargetId.Value);
        });
        sink.RegisterPayloadMapper("AmendmentAppliedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentAppliedEvent)e;
            return new AmendmentAppliedEventSchema(evt.AmendmentId.Value);
        });
        sink.RegisterPayloadMapper("AmendmentRevertedEvent", e =>
        {
            var evt = (DomainEvents.AmendmentRevertedEvent)e;
            return new AmendmentRevertedEventSchema(evt.AmendmentId.Value);
        });
    }
}
