using Whycespace.Shared.Contracts.Events.Trust.Access.Grant;
using DomainEvents = Whycespace.Domain.TrustSystem.Access.Grant;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustAccessGrantSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("GrantIssuedEvent", EventVersion.Default,
            typeof(DomainEvents.GrantIssuedEvent), typeof(GrantIssuedEventSchema));
        sink.RegisterSchema("GrantActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.GrantActivatedEvent), typeof(GrantActivatedEventSchema));
        sink.RegisterSchema("GrantRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.GrantRevokedEvent), typeof(GrantRevokedEventSchema));
        sink.RegisterSchema("GrantExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.GrantExpiredEvent), typeof(GrantExpiredEventSchema));

        sink.RegisterPayloadMapper("GrantIssuedEvent", e =>
        {
            var evt = (DomainEvents.GrantIssuedEvent)e;
            return new GrantIssuedEventSchema(
                evt.GrantId.Value,
                evt.Descriptor.PrincipalReference,
                evt.Descriptor.GrantScope,
                evt.Descriptor.GrantType,
                evt.IssuedAt.Value);
        });
        sink.RegisterPayloadMapper("GrantActivatedEvent", e =>
        {
            var evt = (DomainEvents.GrantActivatedEvent)e;
            return new GrantActivatedEventSchema(evt.GrantId.Value);
        });
        sink.RegisterPayloadMapper("GrantRevokedEvent", e =>
        {
            var evt = (DomainEvents.GrantRevokedEvent)e;
            return new GrantRevokedEventSchema(evt.GrantId.Value);
        });
        sink.RegisterPayloadMapper("GrantExpiredEvent", e =>
        {
            var evt = (DomainEvents.GrantExpiredEvent)e;
            return new GrantExpiredEventSchema(evt.GrantId.Value);
        });
    }
}
