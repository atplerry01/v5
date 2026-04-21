using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Stream;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.StreamCore.Stream;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingStreamCoreStreamSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("StreamCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamCreatedEvent), typeof(StreamCreatedEventSchema));
        sink.RegisterSchema("StreamActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamActivatedEvent), typeof(StreamActivatedEventSchema));
        sink.RegisterSchema("StreamPausedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamPausedEvent), typeof(StreamPausedEventSchema));
        sink.RegisterSchema("StreamResumedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamResumedEvent), typeof(StreamResumedEventSchema));
        sink.RegisterSchema("StreamEndedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamEndedEvent), typeof(StreamEndedEventSchema));
        sink.RegisterSchema("StreamArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.StreamArchivedEvent), typeof(StreamArchivedEventSchema));

        sink.RegisterPayloadMapper("StreamCreatedEvent", e =>
        {
            var evt = (DomainEvents.StreamCreatedEvent)e;
            return new StreamCreatedEventSchema(evt.StreamId.Value, evt.Mode.ToString(), evt.Type.ToString(), evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamActivatedEvent", e =>
        {
            var evt = (DomainEvents.StreamActivatedEvent)e;
            return new StreamActivatedEventSchema(evt.StreamId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamPausedEvent", e =>
        {
            var evt = (DomainEvents.StreamPausedEvent)e;
            return new StreamPausedEventSchema(evt.StreamId.Value, evt.PausedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamResumedEvent", e =>
        {
            var evt = (DomainEvents.StreamResumedEvent)e;
            return new StreamResumedEventSchema(evt.StreamId.Value, evt.ResumedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamEndedEvent", e =>
        {
            var evt = (DomainEvents.StreamEndedEvent)e;
            return new StreamEndedEventSchema(evt.StreamId.Value, evt.EndedAt.Value);
        });
        sink.RegisterPayloadMapper("StreamArchivedEvent", e =>
        {
            var evt = (DomainEvents.StreamArchivedEvent)e;
            return new StreamArchivedEventSchema(evt.StreamId.Value, evt.ArchivedAt.Value);
        });
    }
}
