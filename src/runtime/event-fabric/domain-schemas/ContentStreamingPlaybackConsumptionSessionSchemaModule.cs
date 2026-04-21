using Whycespace.Shared.Contracts.Events.Content.Streaming.PlaybackConsumption.Session;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.PlaybackConsumption.Session;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingPlaybackConsumptionSessionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SessionOpenedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionOpenedEvent), typeof(SessionOpenedEventSchema));
        sink.RegisterSchema("SessionActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionActivatedEvent), typeof(SessionActivatedEventSchema));
        sink.RegisterSchema("SessionSuspendedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionSuspendedEvent), typeof(SessionSuspendedEventSchema));
        sink.RegisterSchema("SessionResumedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionResumedEvent), typeof(SessionResumedEventSchema));
        sink.RegisterSchema("SessionClosedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionClosedEvent), typeof(SessionClosedEventSchema));
        sink.RegisterSchema("SessionFailedEvent", EventVersion.Default,
            typeof(DomainEvents.SessionFailedEvent), typeof(SessionFailedEventSchema));
        sink.RegisterSchema("SessionExpiredEvent", EventVersion.Default,
            typeof(DomainEvents.SessionExpiredEvent), typeof(SessionExpiredEventSchema));

        sink.RegisterPayloadMapper("SessionOpenedEvent", e =>
        {
            var evt = (DomainEvents.SessionOpenedEvent)e;
            return new SessionOpenedEventSchema(
                evt.SessionId.Value,
                evt.StreamRef.Value,
                evt.Window.OpenedAt.Value,
                evt.Window.ExpiresAt.Value);
        });
        sink.RegisterPayloadMapper("SessionActivatedEvent", e =>
        {
            var evt = (DomainEvents.SessionActivatedEvent)e;
            return new SessionActivatedEventSchema(evt.SessionId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("SessionSuspendedEvent", e =>
        {
            var evt = (DomainEvents.SessionSuspendedEvent)e;
            return new SessionSuspendedEventSchema(evt.SessionId.Value, evt.SuspendedAt.Value);
        });
        sink.RegisterPayloadMapper("SessionResumedEvent", e =>
        {
            var evt = (DomainEvents.SessionResumedEvent)e;
            return new SessionResumedEventSchema(evt.SessionId.Value, evt.ResumedAt.Value);
        });
        sink.RegisterPayloadMapper("SessionClosedEvent", e =>
        {
            var evt = (DomainEvents.SessionClosedEvent)e;
            return new SessionClosedEventSchema(evt.SessionId.Value, evt.Reason.Value, evt.ClosedAt.Value);
        });
        sink.RegisterPayloadMapper("SessionFailedEvent", e =>
        {
            var evt = (DomainEvents.SessionFailedEvent)e;
            return new SessionFailedEventSchema(evt.SessionId.Value, evt.Reason.Value, evt.FailedAt.Value);
        });
        sink.RegisterPayloadMapper("SessionExpiredEvent", e =>
        {
            var evt = (DomainEvents.SessionExpiredEvent)e;
            return new SessionExpiredEventSchema(evt.SessionId.Value, evt.ExpiredAt.Value);
        });
    }
}
