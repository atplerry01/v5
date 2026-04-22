using Whycespace.Shared.Contracts.Events.Trust.Access.Session;
using DomainEvents = Whycespace.Domain.TrustSystem.Access.Session;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class TrustAccessSessionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SessionOpenedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionOpenedEvent), typeof(SessionOpenedEventSchema));
        sink.RegisterSchema("SessionExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.SessionExpiredEvent), typeof(SessionExpiredEventSchema));
        sink.RegisterSchema("SessionTerminatedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionTerminatedEvent), typeof(SessionTerminatedEventSchema));

        sink.RegisterPayloadMapper("SessionOpenedEvent", e =>
        {
            var evt = (DomainEvents.SessionOpenedEvent)e;
            return new SessionOpenedEventSchema(
                evt.SessionId.Value,
                evt.Descriptor.IdentityReference,
                evt.Descriptor.SessionContext,
                evt.OpenedAt.Value);
        });
        sink.RegisterPayloadMapper("SessionExpiredEvent", e =>
        {
            var evt = (DomainEvents.SessionExpiredEvent)e;
            return new SessionExpiredEventSchema(evt.SessionId.Value);
        });
        sink.RegisterPayloadMapper("SessionTerminatedEvent", e =>
        {
            var evt = (DomainEvents.SessionTerminatedEvent)e;
            return new SessionTerminatedEventSchema(evt.SessionId.Value);
        });
    }
}
