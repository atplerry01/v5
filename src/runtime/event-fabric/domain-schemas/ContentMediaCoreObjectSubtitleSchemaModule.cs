using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Subtitle;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.CoreObject.Subtitle;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaCoreObjectSubtitleSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("SubtitleCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.SubtitleCreatedEvent), typeof(SubtitleCreatedEventSchema));
        sink.RegisterSchema("SubtitleUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.SubtitleUpdatedEvent), typeof(SubtitleUpdatedEventSchema));
        sink.RegisterSchema("SubtitleFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.SubtitleFinalizedEvent), typeof(SubtitleFinalizedEventSchema));
        sink.RegisterSchema("SubtitleArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.SubtitleArchivedEvent), typeof(SubtitleArchivedEventSchema));

        sink.RegisterPayloadMapper("SubtitleCreatedEvent", e =>
        {
            var evt = (DomainEvents.SubtitleCreatedEvent)e;
            return new SubtitleCreatedEventSchema(
                evt.SubtitleId.Value,
                evt.AssetRef.Value,
                evt.SourceFileRef?.Value,
                evt.Format.ToString(),
                evt.Language.Value,
                evt.Window?.StartMs,
                evt.Window?.EndMs,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("SubtitleUpdatedEvent", e =>
        {
            var evt = (DomainEvents.SubtitleUpdatedEvent)e;
            return new SubtitleUpdatedEventSchema(evt.SubtitleId.Value, evt.OutputRef.Value, evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("SubtitleFinalizedEvent", e =>
        {
            var evt = (DomainEvents.SubtitleFinalizedEvent)e;
            return new SubtitleFinalizedEventSchema(evt.SubtitleId.Value, evt.FinalizedAt.Value);
        });
        sink.RegisterPayloadMapper("SubtitleArchivedEvent", e =>
        {
            var evt = (DomainEvents.SubtitleArchivedEvent)e;
            return new SubtitleArchivedEventSchema(evt.SubtitleId.Value, evt.ArchivedAt.Value);
        });
    }
}
