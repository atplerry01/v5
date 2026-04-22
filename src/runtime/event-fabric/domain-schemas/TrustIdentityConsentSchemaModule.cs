using Whycespace.Shared.Contracts.Events.Trust.Identity.Consent;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Consent;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityConsentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ConsentGrantedEvent", EventVersion.Default,
            typeof(DomainEvents.ConsentGrantedEvent), typeof(ConsentGrantedEventSchema));
        sink.RegisterSchema("ConsentRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.ConsentRevokedEvent), typeof(ConsentRevokedEventSchema));
        sink.RegisterSchema("ConsentExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.ConsentExpiredEvent), typeof(ConsentExpiredEventSchema));

        sink.RegisterPayloadMapper("ConsentGrantedEvent", e =>
        {
            var evt = (DomainEvents.ConsentGrantedEvent)e;
            return new ConsentGrantedEventSchema(
                evt.ConsentId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.ConsentScope,
                evt.Descriptor.ConsentPurpose,
                evt.GrantedAt.Value);
        });
        sink.RegisterPayloadMapper("ConsentRevokedEvent", e =>
        {
            var evt = (DomainEvents.ConsentRevokedEvent)e;
            return new ConsentRevokedEventSchema(evt.ConsentId.Value);
        });
        sink.RegisterPayloadMapper("ConsentExpiredEvent", e =>
        {
            var evt = (DomainEvents.ConsentExpiredEvent)e;
            return new ConsentExpiredEventSchema(evt.ConsentId.Value);
        });
    }
}
