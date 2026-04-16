using Whycespace.Shared.Contracts.Events.Content.Media.Asset;
using DomainEvents = Whycespace.Domain.ContentSystem.Media.Asset;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/media/asset domain. Binds domain event CLR
/// types to <see cref="EventSchemaRegistry"/> and registers payload mappers
/// that project domain events into shared-contract schemas for Kafka transport.
/// </summary>
public sealed class MediaAssetSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("MediaAssetRegisteredEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetRegisteredEvent), typeof(MediaAssetRegisteredEventSchema));
        sink.RegisterSchema("MediaAssetProcessingStartedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetProcessingStartedEvent), typeof(MediaAssetProcessingStartedEventSchema));
        sink.RegisterSchema("MediaAssetAvailableEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetAvailableEvent), typeof(MediaAssetAvailableEventSchema));
        sink.RegisterSchema("MediaAssetPublishedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetPublishedEvent), typeof(MediaAssetPublishedEventSchema));
        sink.RegisterSchema("MediaAssetUnpublishedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetUnpublishedEvent), typeof(MediaAssetUnpublishedEventSchema));
        sink.RegisterSchema("MediaAssetArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetArchivedEvent), typeof(MediaAssetArchivedEventSchema));
        sink.RegisterSchema("MediaAssetMetadataUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.MediaAssetMetadataUpdatedEvent), typeof(MediaAssetMetadataUpdatedEventSchema));

        sink.RegisterPayloadMapper("MediaAssetRegisteredEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetRegisteredEvent)e;
            return new MediaAssetRegisteredEventSchema(
                evt.AggregateId.Value,
                evt.MediaAssetId.Value,
                evt.OwnerRef,
                (int)evt.MediaType,
                evt.Title,
                evt.Description,
                evt.ContentDigest,
                evt.StorageUri,
                evt.StorageSizeBytes,
                evt.RegisteredAt.Value);
        });

        sink.RegisterPayloadMapper("MediaAssetProcessingStartedEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetProcessingStartedEvent)e;
            return new MediaAssetProcessingStartedEventSchema(evt.AggregateId.Value, evt.MediaAssetId.Value, evt.StartedAt.Value);
        });

        sink.RegisterPayloadMapper("MediaAssetAvailableEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetAvailableEvent)e;
            return new MediaAssetAvailableEventSchema(evt.AggregateId.Value, evt.MediaAssetId.Value, evt.AvailableAt.Value);
        });

        sink.RegisterPayloadMapper("MediaAssetPublishedEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetPublishedEvent)e;
            return new MediaAssetPublishedEventSchema(evt.AggregateId.Value, evt.MediaAssetId.Value, evt.PublishedAt.Value);
        });

        sink.RegisterPayloadMapper("MediaAssetUnpublishedEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetUnpublishedEvent)e;
            return new MediaAssetUnpublishedEventSchema(evt.AggregateId.Value, evt.MediaAssetId.Value, evt.UnpublishedAt.Value);
        });

        sink.RegisterPayloadMapper("MediaAssetArchivedEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetArchivedEvent)e;
            return new MediaAssetArchivedEventSchema(evt.AggregateId.Value, evt.MediaAssetId.Value, evt.ArchivedAt.Value);
        });

        sink.RegisterPayloadMapper("MediaAssetMetadataUpdatedEvent", e =>
        {
            var evt = (DomainEvents.MediaAssetMetadataUpdatedEvent)e;
            return new MediaAssetMetadataUpdatedEventSchema(
                evt.AggregateId.Value, evt.MediaAssetId.Value,
                evt.Title, evt.Description, evt.Tags, evt.UpdatedAt.Value);
        });
    }
}
