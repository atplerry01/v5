using Whycespace.Shared.Contracts.Events.Trust.Access.Authorization;
using DomainEvents = Whycespace.Domain.TrustSystem.Access.Authorization;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustAccessAuthorizationSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("AuthorizationGrantedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorizationGrantedEvent), typeof(AuthorizationGrantedEventSchema));
        sink.RegisterSchema("AuthorizationDeniedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorizationDeniedEvent), typeof(AuthorizationDeniedEventSchema));
        sink.RegisterSchema("AuthorizationRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.AuthorizationRevokedEvent), typeof(AuthorizationRevokedEventSchema));

        sink.RegisterPayloadMapper("AuthorizationGrantedEvent", e =>
        {
            var evt = (DomainEvents.AuthorizationGrantedEvent)e;
            return new AuthorizationGrantedEventSchema(
                evt.AuthorizationId.Value,
                evt.Scope.PrincipalReference,
                evt.Scope.ResourceReference);
        });
        sink.RegisterPayloadMapper("AuthorizationDeniedEvent", e =>
        {
            var evt = (DomainEvents.AuthorizationDeniedEvent)e;
            return new AuthorizationDeniedEventSchema(
                evt.AuthorizationId.Value,
                evt.Scope.PrincipalReference,
                evt.Scope.ResourceReference);
        });
        sink.RegisterPayloadMapper("AuthorizationRevokedEvent", e =>
        {
            var evt = (DomainEvents.AuthorizationRevokedEvent)e;
            return new AuthorizationRevokedEventSchema(evt.AuthorizationId.Value);
        });
    }
}
