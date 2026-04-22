using Whycespace.Shared.Contracts.Events.Trust.Identity.Identity;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Identity;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityIdentitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("IdentityEstablishedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentityEstablishedEvent), typeof(IdentityEstablishedEventSchema));
        sink.RegisterSchema("IdentitySuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentitySuspendedEvent), typeof(IdentitySuspendedEventSchema));
        sink.RegisterSchema("IdentityTerminatedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentityTerminatedEvent), typeof(IdentityTerminatedEventSchema));

        sink.RegisterPayloadMapper("IdentityEstablishedEvent", e =>
        {
            var evt = (DomainEvents.IdentityEstablishedEvent)e;
            return new IdentityEstablishedEventSchema(
                evt.IdentityId.Value,
                evt.Descriptor.PrincipalName,
                evt.Descriptor.PrincipalType);
        });
        sink.RegisterPayloadMapper("IdentitySuspendedEvent", e =>
        {
            var evt = (DomainEvents.IdentitySuspendedEvent)e;
            return new IdentitySuspendedEventSchema(evt.IdentityId.Value);
        });
        sink.RegisterPayloadMapper("IdentityTerminatedEvent", e =>
        {
            var evt = (DomainEvents.IdentityTerminatedEvent)e;
            return new IdentityTerminatedEventSchema(evt.IdentityId.Value);
        });
    }
}
