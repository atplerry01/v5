using Whycespace.Shared.Contracts.Events.Control.AccessControl.Identity;
using DomainEvents = Whycespace.Domain.ControlSystem.AccessControl.Identity;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAccessControlIdentitySchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("IdentityRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.IdentityRegisteredEvent), typeof(IdentityRegisteredEventSchema));
        sink.RegisterSchema("IdentitySuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentitySuspendedEvent), typeof(IdentitySuspendedEventSchema));
        sink.RegisterSchema("IdentityDeactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.IdentityDeactivatedEvent), typeof(IdentityDeactivatedEventSchema));

        sink.RegisterPayloadMapper("IdentityRegisteredEvent", e =>
        {
            var evt = (DomainEvents.IdentityRegisteredEvent)e;
            return new IdentityRegisteredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Kind.ToString());
        });
        sink.RegisterPayloadMapper("IdentitySuspendedEvent", e =>
        {
            var evt = (DomainEvents.IdentitySuspendedEvent)e;
            return new IdentitySuspendedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Reason);
        });
        sink.RegisterPayloadMapper("IdentityDeactivatedEvent", e =>
        {
            var evt = (DomainEvents.IdentityDeactivatedEvent)e;
            return new IdentityDeactivatedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
