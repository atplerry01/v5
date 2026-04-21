using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Transcript;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.CoreObject.Transcript;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaCoreObjectTranscriptSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("TranscriptCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.TranscriptCreatedEvent), typeof(TranscriptCreatedEventSchema));
        sink.RegisterSchema("TranscriptUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.TranscriptUpdatedEvent), typeof(TranscriptUpdatedEventSchema));
        sink.RegisterSchema("TranscriptFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.TranscriptFinalizedEvent), typeof(TranscriptFinalizedEventSchema));
        sink.RegisterSchema("TranscriptArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.TranscriptArchivedEvent), typeof(TranscriptArchivedEventSchema));

        sink.RegisterPayloadMapper("TranscriptCreatedEvent", e =>
        {
            var evt = (DomainEvents.TranscriptCreatedEvent)e;
            return new TranscriptCreatedEventSchema(
                evt.TranscriptId.Value,
                evt.AssetRef.Value,
                evt.SourceFileRef?.Value,
                evt.Format.ToString(),
                evt.Language.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("TranscriptUpdatedEvent", e =>
        {
            var evt = (DomainEvents.TranscriptUpdatedEvent)e;
            return new TranscriptUpdatedEventSchema(evt.TranscriptId.Value, evt.OutputRef.Value, evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("TranscriptFinalizedEvent", e =>
        {
            var evt = (DomainEvents.TranscriptFinalizedEvent)e;
            return new TranscriptFinalizedEventSchema(evt.TranscriptId.Value, evt.FinalizedAt.Value);
        });
        sink.RegisterPayloadMapper("TranscriptArchivedEvent", e =>
        {
            var evt = (DomainEvents.TranscriptArchivedEvent)e;
            return new TranscriptArchivedEventSchema(evt.TranscriptId.Value, evt.ArchivedAt.Value);
        });
    }
}
