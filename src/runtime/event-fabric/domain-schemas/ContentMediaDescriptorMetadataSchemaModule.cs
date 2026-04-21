using Whycespace.Shared.Contracts.Events.Content.Media.Descriptor.Metadata;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

public sealed class ContentMediaDescriptorMetadataSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MediaMetadataCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaMetadataCreatedEvent), typeof(MediaMetadataCreatedEventSchema));
        sink.RegisterSchema("MediaMetadataEntryAddedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaMetadataEntryAddedEvent), typeof(MediaMetadataEntryAddedEventSchema));
        sink.RegisterSchema("MediaMetadataEntryUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaMetadataEntryUpdatedEvent), typeof(MediaMetadataEntryUpdatedEventSchema));
        sink.RegisterSchema("MediaMetadataEntryRemovedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaMetadataEntryRemovedEvent), typeof(MediaMetadataEntryRemovedEventSchema));
        sink.RegisterSchema("MediaMetadataFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaMetadataFinalizedEvent), typeof(MediaMetadataFinalizedEventSchema));

        sink.RegisterPayloadMapper("MediaMetadataCreatedEvent", e =>
        {
            var evt = (DomainEvents.MediaMetadataCreatedEvent)e;
            return new MediaMetadataCreatedEventSchema(evt.MetadataId.Value, evt.AssetRef.Value, evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaMetadataEntryAddedEvent", e =>
        {
            var evt = (DomainEvents.MediaMetadataEntryAddedEvent)e;
            return new MediaMetadataEntryAddedEventSchema(evt.MetadataId.Value, evt.Key.Value, evt.Value.Value, evt.AddedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaMetadataEntryUpdatedEvent", e =>
        {
            var evt = (DomainEvents.MediaMetadataEntryUpdatedEvent)e;
            return new MediaMetadataEntryUpdatedEventSchema(evt.MetadataId.Value, evt.Key.Value, evt.PreviousValue.Value, evt.NewValue.Value, evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaMetadataEntryRemovedEvent", e =>
        {
            var evt = (DomainEvents.MediaMetadataEntryRemovedEvent)e;
            return new MediaMetadataEntryRemovedEventSchema(evt.MetadataId.Value, evt.Key.Value, evt.RemovedAt.Value);
        });
        sink.RegisterPayloadMapper("MediaMetadataFinalizedEvent", e =>
        {
            var evt = (DomainEvents.MediaMetadataFinalizedEvent)e;
            return new MediaMetadataFinalizedEventSchema(evt.MetadataId.Value, evt.FinalizedAt.Value);
        });
    }
}
