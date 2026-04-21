using Whycespace.Shared.Contracts.Events.Content.Streaming.LiveStreaming.Archive;
using DomainEvents = Whycespace.Domain.ContentSystem.Streaming.LiveStreaming.Archive;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentStreamingLiveStreamingArchiveSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("ArchiveStartedEvent", EventVersion.Default,
            typeof(DomainEvents.ArchiveStartedEvent), typeof(ArchiveStartedEventSchema));
        sink.RegisterSchema("ArchiveCompletedEvent", EventVersion.Default,
            typeof(DomainEvents.ArchiveCompletedEvent), typeof(ArchiveCompletedEventSchema));
        sink.RegisterSchema("ArchiveFailedEvent", EventVersion.Default,
            typeof(DomainEvents.ArchiveFailedEvent), typeof(ArchiveFailedEventSchema));
        sink.RegisterSchema("ArchiveFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.ArchiveFinalizedEvent), typeof(ArchiveFinalizedEventSchema));
        sink.RegisterSchema("ArchiveArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.ArchiveArchivedEvent), typeof(ArchiveArchivedEventSchema));

        sink.RegisterPayloadMapper("ArchiveStartedEvent", e =>
        {
            var evt = (DomainEvents.ArchiveStartedEvent)e;
            return new ArchiveStartedEventSchema(
                evt.ArchiveId.Value,
                evt.StreamRef.Value,
                evt.SessionRef?.Value,
                evt.StartedAt.Value);
        });
        sink.RegisterPayloadMapper("ArchiveCompletedEvent", e =>
        {
            var evt = (DomainEvents.ArchiveCompletedEvent)e;
            return new ArchiveCompletedEventSchema(evt.ArchiveId.Value, evt.OutputRef.Value, evt.CompletedAt.Value);
        });
        sink.RegisterPayloadMapper("ArchiveFailedEvent", e =>
        {
            var evt = (DomainEvents.ArchiveFailedEvent)e;
            return new ArchiveFailedEventSchema(evt.ArchiveId.Value, evt.Reason.Value, evt.FailedAt.Value);
        });
        sink.RegisterPayloadMapper("ArchiveFinalizedEvent", e =>
        {
            var evt = (DomainEvents.ArchiveFinalizedEvent)e;
            return new ArchiveFinalizedEventSchema(evt.ArchiveId.Value, evt.FinalizedAt.Value);
        });
        sink.RegisterPayloadMapper("ArchiveArchivedEvent", e =>
        {
            var evt = (DomainEvents.ArchiveArchivedEvent)e;
            return new ArchiveArchivedEventSchema(evt.ArchiveId.Value, evt.ArchivedAt.Value);
        });
    }
}
