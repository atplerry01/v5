using Whycespace.Shared.Contracts.Events.Control.AccessControl.Role;
using DomainEvents = Whycespace.Domain.ControlSystem.AccessControl.Role;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAccessControlRoleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RoleDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.RoleDefinedEvent), typeof(RoleDefinedEventSchema));
        sink.RegisterSchema("RolePermissionAddedEvent", EventVersion.Default,
            typeof(DomainEvents.RolePermissionAddedEvent), typeof(RolePermissionAddedEventSchema));
        sink.RegisterSchema("RoleDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.RoleDeprecatedEvent), typeof(RoleDeprecatedEventSchema));

        sink.RegisterPayloadMapper("RoleDefinedEvent", e =>
        {
            var evt = (DomainEvents.RoleDefinedEvent)e;
            return new RoleDefinedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.PermissionIds.ToList(),
                evt.ParentRoleId);
        });
        sink.RegisterPayloadMapper("RolePermissionAddedEvent", e =>
        {
            var evt = (DomainEvents.RolePermissionAddedEvent)e;
            return new RolePermissionAddedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.PermissionId);
        });
        sink.RegisterPayloadMapper("RoleDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.RoleDeprecatedEvent)e;
            return new RoleDeprecatedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
