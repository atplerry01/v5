using Whycespace.Shared.Contracts.Events.Trust.Identity.ServiceIdentity;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityServiceIdentitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ServiceIdentityRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.ServiceIdentityRegisteredEvent), typeof(ServiceIdentityRegisteredEventSchema));
        sink.RegisterSchema("ServiceIdentitySuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.ServiceIdentitySuspendedEvent), typeof(ServiceIdentitySuspendedEventSchema));
        sink.RegisterSchema("ServiceIdentityDecommissionedEvent", EventVersion.Default,
            typeof(DomainEvents.ServiceIdentityDecommissionedEvent), typeof(ServiceIdentityDecommissionedEventSchema));

        sink.RegisterPayloadMapper("ServiceIdentityRegisteredEvent", e =>
        {
            var evt = (DomainEvents.ServiceIdentityRegisteredEvent)e;
            return new ServiceIdentityRegisteredEventSchema(
                evt.ServiceIdentityId.Value,
                evt.Descriptor.OwnerReference,
                evt.Descriptor.ServiceName,
                evt.Descriptor.ServiceType,
                evt.RegisteredAt.Value);
        });
        sink.RegisterPayloadMapper("ServiceIdentitySuspendedEvent", e =>
        {
            var evt = (DomainEvents.ServiceIdentitySuspendedEvent)e;
            return new ServiceIdentitySuspendedEventSchema(evt.ServiceIdentityId.Value);
        });
        sink.RegisterPayloadMapper("ServiceIdentityDecommissionedEvent", e =>
        {
            var evt = (DomainEvents.ServiceIdentityDecommissionedEvent)e;
            return new ServiceIdentityDecommissionedEventSchema(evt.ServiceIdentityId.Value);
        });
    }
}
