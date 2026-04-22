using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.IngestSession;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.IngestSession;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingLiveStreamingIngestSessionSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("IngestSessionAuthenticatedEvent", EventVersion.Default,
            typeof(DomainEvents.IngestSessionAuthenticatedEvent), typeof(IngestSessionAuthenticatedEventSchema));
        sink.RegisterSchema("IngestStreamingStartedEvent", EventVersion.Default,
            typeof(DomainEvents.IngestStreamingStartedEvent), typeof(IngestStreamingStartedEventSchema));
        sink.RegisterSchema("IngestSessionStalledEvent", EventVersion.Default,
            typeof(DomainEvents.IngestSessionStalledEvent), typeof(IngestSessionStalledEventSchema));
        sink.RegisterSchema("IngestSessionResumedEvent", EventVersion.Default,
            typeof(DomainEvents.IngestSessionResumedEvent), typeof(IngestSessionResumedEventSchema));
        sink.RegisterSchema("IngestSessionEndedEvent", EventVersion.Default,
            typeof(DomainEvents.IngestSessionEndedEvent), typeof(IngestSessionEndedEventSchema));
        sink.RegisterSchema("IngestSessionFailedEvent", EventVersion.Default,
            typeof(DomainEvents.IngestSessionFailedEvent), typeof(IngestSessionFailedEventSchema));

        sink.RegisterPayloadMapper("IngestSessionAuthenticatedEvent", e =>
        {
            var evt = (DomainEvents.IngestSessionAuthenticatedEvent)e;
            return new IngestSessionAuthenticatedEventSchema(evt.SessionId.Value, evt.BroadcastRef.Value, evt.Endpoint.Value, evt.AuthenticatedAt.Value);
        });
        sink.RegisterPayloadMapper("IngestStreamingStartedEvent", e =>
        {
            var evt = (DomainEvents.IngestStreamingStartedEvent)e;
            return new IngestStreamingStartedEventSchema(evt.SessionId.Value, evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("IngestSessionStalledEvent", e =>
        {
            var evt = (DomainEvents.IngestSessionStalledEvent)e;
            return new IngestSessionStalledEventSchema(evt.SessionId.Value, evt.StalledAt.Value);
        });
        sink.RegisterPayloadMapper("IngestSessionResumedEvent", e =>
        {
            var evt = (DomainEvents.IngestSessionResumedEvent)e;
            return new IngestSessionResumedEventSchema(evt.SessionId.Value, evt.ResumedAt.Value);
        });
        sink.RegisterPayloadMapper("IngestSessionEndedEvent", e =>
        {
            var evt = (DomainEvents.IngestSessionEndedEvent)e;
            return new IngestSessionEndedEventSchema(evt.SessionId.Value, evt.EndedAt.Value);
        });
        sink.RegisterPayloadMapper("IngestSessionFailedEvent", e =>
        {
            var evt = (DomainEvents.IngestSessionFailedEvent)e;
            return new IngestSessionFailedEventSchema(evt.SessionId.Value, evt.FailureReason, evt.FailedAt.Value);
        });
    }
}
