using Whycespace.Shared.Contracts.Events.Trust.Access.Role;
using DomainEvents = Whycespace.Domain.TrustSystem.Access.Role;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustAccessRoleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("RoleDefinedEvent", EventVersion.Default,
            typeof(DomainEvents.RoleDefinedEvent), typeof(RoleDefinedEventSchema));
        sink.RegisterSchema("RoleActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.RoleActivatedEvent), typeof(RoleActivatedEventSchema));
        sink.RegisterSchema("RoleDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.RoleDeprecatedEvent), typeof(RoleDeprecatedEventSchema));

        sink.RegisterPayloadMapper("RoleDefinedEvent", e =>
        {
            var evt = (DomainEvents.RoleDefinedEvent)e;
            return new RoleDefinedEventSchema(
                evt.RoleId.Value,
                evt.Descriptor.RoleName,
                evt.Descriptor.RoleScope,
                evt.DefinedAt.Value);
        });
        sink.RegisterPayloadMapper("RoleActivatedEvent", e =>
        {
            var evt = (DomainEvents.RoleActivatedEvent)e;
            return new RoleActivatedEventSchema(evt.RoleId.Value);
        });
        sink.RegisterPayloadMapper("RoleDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.RoleDeprecatedEvent)e;
            return new RoleDeprecatedEventSchema(evt.RoleId.Value);
        });
    }
}
