using Whycespace.Shared.Contracts.Events.Control.AccessControl.Authorization;
using DomainEvents = Whycespace.Domain.ControlSystem.AccessControl.Authorization;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ControlAccessControlAuthorizationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuthorizationGrantedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorizationGrantedEvent), typeof(AuthorizationGrantedEventSchema));
        sink.RegisterSchema("AuthorizationRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorizationRevokedEvent), typeof(AuthorizationRevokedEventSchema));

        sink.RegisterPayloadMapper("AuthorizationGrantedEvent", e =>
        {
            var evt = (DomainEvents.AuthorizationGrantedEvent)e;
            return new AuthorizationGrantedEventSchema(
                Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"),
                evt.SubjectId,
                evt.RoleIds.ToList(),
                evt.ValidFrom,
                evt.ValidTo);
        });
        sink.RegisterPayloadMapper("AuthorizationRevokedEvent", e =>
        {
            var evt = (DomainEvents.AuthorizationRevokedEvent)e;
            return new AuthorizationRevokedEventSchema(Guid.ParseExact(evt.Id.Value.Substring(0, 32), "N"));
        });
    }
}
