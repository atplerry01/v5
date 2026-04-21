using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Renewal;
using DomainEvents = Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Renewal;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class BusinessAgreementChangeControlRenewalSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema(
            "RenewalCreatedEvent",
            EventVersion.Default,
            typeof(DomainEvents.RenewalCreatedEvent),
            typeof(RenewalCreatedEventSchema));

        sink.RegisterSchema(
            "RenewalRenewedEvent",
            EventVersion.Default,
            typeof(DomainEvents.RenewalRenewedEvent),
            typeof(RenewalRenewedEventSchema));

        sink.RegisterSchema(
            "RenewalExpiredEvent",
            EventVersion.Default,
            typeof(DomainEvents.RenewalExpiredEvent),
            typeof(RenewalExpiredEventSchema));

        sink.RegisterPayloadMapper("RenewalCreatedEvent", e =>
        {
            var evt = (DomainEvents.RenewalCreatedEvent)e;
            return new RenewalCreatedEventSchema(evt.RenewalId.Value, evt.SourceId.Value);
        });
        sink.RegisterPayloadMapper("RenewalRenewedEvent", e =>
        {
            var evt = (DomainEvents.RenewalRenewedEvent)e;
            return new RenewalRenewedEventSchema(evt.RenewalId.Value);
        });
        sink.RegisterPayloadMapper("RenewalExpiredEvent", e =>
        {
            var evt = (DomainEvents.RenewalExpiredEvent)e;
            return new RenewalExpiredEventSchema(evt.RenewalId.Value);
        });
    }
}
