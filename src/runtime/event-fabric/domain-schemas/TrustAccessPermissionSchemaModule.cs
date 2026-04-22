using Whycespace.Shared.Contracts.Events.Trust.Access.Permission;
using DomainEvents = Whycespace.Domain.TrustSystem.Access.Permission;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustAccessPermissionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PermissionDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.PermissionDefinedEvent), typeof(PermissionDefinedEventSchema));
        sink.RegisterSchema("PermissionActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.PermissionActivatedEvent), typeof(PermissionActivatedEventSchema));
        sink.RegisterSchema("PermissionDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.PermissionDeprecatedEvent), typeof(PermissionDeprecatedEventSchema));

        sink.RegisterPayloadMapper("PermissionDefinedEvent", e =>
        {
            var evt = (DomainEvents.PermissionDefinedEvent)e;
            return new PermissionDefinedEventSchema(
                evt.PermissionId.Value,
                evt.Descriptor.PermissionName,
                evt.Descriptor.ResourceType);
        });
        sink.RegisterPayloadMapper("PermissionActivatedEvent", e =>
        {
            var evt = (DomainEvents.PermissionActivatedEvent)e;
            return new PermissionActivatedEventSchema(evt.PermissionId.Value);
        });
        sink.RegisterPayloadMapper("PermissionDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.PermissionDeprecatedEvent)e;
            return new PermissionDeprecatedEventSchema(evt.PermissionId.Value);
        });
    }
}
