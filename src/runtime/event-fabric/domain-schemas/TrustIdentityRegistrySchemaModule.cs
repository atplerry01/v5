using Whycespace.Shared.Contracts.Events.Trust.Identity.Registry;
using DomainEvents = Whycespace.Domain.TrustSystem.Identity.Registry;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustIdentityRegistrySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RegistrationInitiatedEvent", EventVersion.Default,
            typeof(DomainEvents.RegistrationInitiatedEvent), typeof(RegistrationInitiatedEventSchema));
        sink.RegisterSchema("RegistrationVerifiedEvent", EventVersion.Default,
            typeof(DomainEvents.RegistrationVerifiedEvent), typeof(RegistrationVerifiedEventSchema));
        sink.RegisterSchema("RegistrationActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.RegistrationActivatedEvent), typeof(RegistrationActivatedEventSchema));
        sink.RegisterSchema("RegistrationRejectedEvent", EventVersion.Default,
            typeof(DomainEvents.RegistrationRejectedEvent), typeof(RegistrationRejectedEventSchema));
        sink.RegisterSchema("RegistrationLockedEvent", EventVersion.Default,
            typeof(DomainEvents.RegistrationLockedEvent), typeof(RegistrationLockedEventSchema));

        sink.RegisterPayloadMapper("RegistrationInitiatedEvent", e =>
        {
            var evt = (DomainEvents.RegistrationInitiatedEvent)e;
            return new RegistrationInitiatedEventSchema(
                evt.RegistryId.Value,
                evt.Descriptor.Email,
                evt.Descriptor.RegistrationType,
                evt.InitiatedAt.Value);
        });
        sink.RegisterPayloadMapper("RegistrationVerifiedEvent", e =>
        {
            var evt = (DomainEvents.RegistrationVerifiedEvent)e;
            return new RegistrationVerifiedEventSchema(evt.RegistryId.Value);
        });
        sink.RegisterPayloadMapper("RegistrationActivatedEvent", e =>
        {
            var evt = (DomainEvents.RegistrationActivatedEvent)e;
            return new RegistrationActivatedEventSchema(evt.RegistryId.Value);
        });
        sink.RegisterPayloadMapper("RegistrationRejectedEvent", e =>
        {
            var evt = (DomainEvents.RegistrationRejectedEvent)e;
            return new RegistrationRejectedEventSchema(evt.RegistryId.Value, evt.Reason);
        });
        sink.RegisterPayloadMapper("RegistrationLockedEvent", e =>
        {
            var evt = (DomainEvents.RegistrationLockedEvent)e;
            return new RegistrationLockedEventSchema(evt.RegistryId.Value, evt.Reason);
        });
    }
}
