using Whycespace.Shared.Contracts.Events.Control.AccessControl.Principal;
using DomainEvents = Whycespace.Domain.ControlSystem.AccessControl.Principal;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAccessControlPrincipalSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("PrincipalRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.PrincipalRegisteredEvent), typeof(PrincipalRegisteredEventSchema));
        sink.RegisterSchema("PrincipalRoleAssignedEvent", EventVersion.Default,
            typeof(DomainEvents.PrincipalRoleAssignedEvent), typeof(PrincipalRoleAssignedEventSchema));
        sink.RegisterSchema("PrincipalDeactivatedEvent", EventVersion.Default,
            typeof(DomainEvents.PrincipalDeactivatedEvent), typeof(PrincipalDeactivatedEventSchema));

        sink.RegisterPayloadMapper("PrincipalRegisteredEvent", e =>
        {
            var evt = (DomainEvents.PrincipalRegisteredEvent)e;
            return new PrincipalRegisteredEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.Name,
                evt.Kind.ToString(),
                evt.IdentityId);
        });
        sink.RegisterPayloadMapper("PrincipalRoleAssignedEvent", e =>
        {
            var evt = (DomainEvents.PrincipalRoleAssignedEvent)e;
            return new PrincipalRoleAssignedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.RoleId);
        });
        sink.RegisterPayloadMapper("PrincipalDeactivatedEvent", e =>
        {
            var evt = (DomainEvents.PrincipalDeactivatedEvent)e;
            return new PrincipalDeactivatedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
