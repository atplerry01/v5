using Whycespace.Shared.Contracts.Events.Control.AccessControl.Permission;
using DomainEvents = Whycespace.Domain.ControlSystem.AccessControl.Permission;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAccessControlPermissionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PermissionDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.PermissionDefinedEvent), typeof(PermissionDefinedEventSchema));
        sink.RegisterSchema("PermissionDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.PermissionDeprecatedEvent), typeof(PermissionDeprecatedEventSchema));

        sink.RegisterPayloadMapper("PermissionDefinedEvent", e =>
        {
            var evt = (DomainEvents.PermissionDefinedEvent)e;
            return new PermissionDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.ResourceScope,
                evt.Actions.ToString());
        });
        sink.RegisterPayloadMapper("PermissionDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.PermissionDeprecatedEvent)e;
            return new PermissionDeprecatedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
