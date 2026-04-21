using Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.Access;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingDeliveryGovernanceAccessSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("StreamAccessGrantedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamAccessGrantedEvent), typeof(StreamAccessGrantedEventSchema));
        sink.RegisterSchema("StreamAccessRestrictedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamAccessRestrictedEvent), typeof(StreamAccessRestrictedEventSchema));
        sink.RegisterSchema("StreamAccessUnrestrictedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamAccessUnrestrictedEvent), typeof(StreamAccessUnrestrictedEventSchema));
        sink.RegisterSchema("StreamAccessRevokedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamAccessRevokedEvent), typeof(StreamAccessRevokedEventSchema));
        sink.RegisterSchema("StreamAccessExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.StreamAccessExpiredEvent), typeof(StreamAccessExpiredEventSchema));

        sink.RegisterPayloadMapper("StreamAccessGrantedEvent", e =>
        {
            var evt = (DomainEvents.StreamAccessGrantedEvent)e;
            return new StreamAccessGrantedEventSchema(
                evt.AccessId.Value,
                evt.StreamRef.Value,
                evt.Mode.ToString(),
                evt.Window.Start.Value,
                evt.Window.End.Value,
                evt.Token.Value,
                evt.GrantedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamAccessRestrictedEvent", e =>
        {
            var evt = (DomainEvents.StreamAccessRestrictedEvent)e;
            return new StreamAccessRestrictedEventSchema(evt.AccessId.Value, evt.Reason, evt.RestrictedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamAccessUnrestrictedEvent", e =>
        {
            var evt = (DomainEvents.StreamAccessUnrestrictedEvent)e;
            return new StreamAccessUnrestrictedEventSchema(evt.AccessId.Value, evt.UnrestrictedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamAccessRevokedEvent", e =>
        {
            var evt = (DomainEvents.StreamAccessRevokedEvent)e;
            return new StreamAccessRevokedEventSchema(evt.AccessId.Value, evt.Reason, evt.RevokedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamAccessExpiredEvent", e =>
        {
            var evt = (DomainEvents.StreamAccessExpiredEvent)e;
            return new StreamAccessExpiredEventSchema(evt.AccessId.Value, evt.ExpiredAt.Value);
        });
    }
}
