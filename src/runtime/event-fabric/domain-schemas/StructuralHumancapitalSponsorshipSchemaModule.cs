using Whycespace.Shared.Contracts.Events.Structural.Humancapital.Sponsorship;
using DomainEvents = Whycespace.Domain.BusinessSystem.Workforce.Sponsorship;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class StructuralHumancapitalSponsorshipSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SponsorshipCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.SponsorshipCreatedEvent), typeof(SponsorshipCreatedEventSchema));

        sink.RegisterPayloadMapper("SponsorshipCreatedEvent", e =>
        {
            var evt = (DomainEvents.SponsorshipCreatedEvent)e;
            return new SponsorshipCreatedEventSchema(evt.SponsorshipId.Value, evt.Descriptor.Name, evt.Descriptor.Kind);
        });
    }
}
