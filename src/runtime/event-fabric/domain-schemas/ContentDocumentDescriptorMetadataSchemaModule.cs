using Whycespace.Shared.Contracts.Events.Content.Document.Descriptor.Metadata;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/descriptor/metadata BC. Owns
/// binding from domain event CLR types to the <see cref="EventSchemaRegistry"/>
/// plus outbound payload mappers that project domain events into shared
/// schema records for the projection layer.
/// </summary>
public sealed class ContentDocumentDescriptorMetadataSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentMetadataCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentMetadataCreatedEvent), typeof(DocumentMetadataCreatedEventSchema));
        sink.RegisterSchema("DocumentMetadataEntryAddedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentMetadataEntryAddedEvent), typeof(DocumentMetadataEntryAddedEventSchema));
        sink.RegisterSchema("DocumentMetadataEntryUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentMetadataEntryUpdatedEvent), typeof(DocumentMetadataEntryUpdatedEventSchema));
        sink.RegisterSchema("DocumentMetadataEntryRemovedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentMetadataEntryRemovedEvent), typeof(DocumentMetadataEntryRemovedEventSchema));
        sink.RegisterSchema("DocumentMetadataFinalizedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentMetadataFinalizedEvent), typeof(DocumentMetadataFinalizedEventSchema));

        sink.RegisterPayloadMapper("DocumentMetadataCreatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentMetadataCreatedEvent)e;
            return new DocumentMetadataCreatedEventSchema(
                evt.MetadataId.Value,
                evt.DocumentRef.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentMetadataEntryAddedEvent", e =>
        {
            var evt = (DomainEvents.DocumentMetadataEntryAddedEvent)e;
            return new DocumentMetadataEntryAddedEventSchema(
                evt.MetadataId.Value,
                evt.Key.Value,
                evt.Value.Value,
                evt.AddedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentMetadataEntryUpdatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentMetadataEntryUpdatedEvent)e;
            return new DocumentMetadataEntryUpdatedEventSchema(
                evt.MetadataId.Value,
                evt.Key.Value,
                evt.PreviousValue.Value,
                evt.NewValue.Value,
                evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentMetadataEntryRemovedEvent", e =>
        {
            var evt = (DomainEvents.DocumentMetadataEntryRemovedEvent)e;
            return new DocumentMetadataEntryRemovedEventSchema(
                evt.MetadataId.Value,
                evt.Key.Value,
                evt.RemovedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentMetadataFinalizedEvent", e =>
        {
            var evt = (DomainEvents.DocumentMetadataFinalizedEvent)e;
            return new DocumentMetadataFinalizedEventSchema(
                evt.MetadataId.Value,
                evt.FinalizedAt.Value);
        });
    }
}
