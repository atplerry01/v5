using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Document;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.CoreObject.Document;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/core-object/document BC. Owns
/// binding from domain event CLR types to the <see cref="EventSchemaRegistry"/>
/// plus outbound payload mappers that project domain events into shared
/// schema records for the projection layer.
/// </summary>
public sealed class ContentDocumentCoreObjectDocumentSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentCreatedEvent), typeof(DocumentCreatedEventSchema));
        sink.RegisterSchema("DocumentMetadataUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentMetadataUpdatedEvent), typeof(DocumentMetadataUpdatedEventSchema));
        sink.RegisterSchema("DocumentVersionAttachedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentVersionAttachedEvent), typeof(DocumentVersionAttachedEventSchema));
        sink.RegisterSchema("DocumentActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentActivatedEvent), typeof(DocumentActivatedEventSchema));
        sink.RegisterSchema("DocumentArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentArchivedEvent), typeof(DocumentArchivedEventSchema));
        sink.RegisterSchema("DocumentRestoredEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentRestoredEvent), typeof(DocumentRestoredEventSchema));
        sink.RegisterSchema("DocumentSupersededEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentSupersededEvent), typeof(DocumentSupersededEventSchema));

        sink.RegisterPayloadMapper("DocumentCreatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentCreatedEvent)e;
            return new DocumentCreatedEventSchema(
                evt.DocumentId.Value,
                evt.Title.Value,
                evt.Type.ToString(),
                evt.Classification.ToString(),
                evt.StructuralOwner.Value,
                evt.BusinessOwner.Kind.ToString(),
                evt.BusinessOwner.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentMetadataUpdatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentMetadataUpdatedEvent)e;
            return new DocumentMetadataUpdatedEventSchema(
                evt.DocumentId.Value,
                evt.PreviousTitle.Value,
                evt.NewTitle.Value,
                evt.PreviousType.ToString(),
                evt.NewType.ToString(),
                evt.PreviousClassification.ToString(),
                evt.NewClassification.ToString(),
                evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentVersionAttachedEvent", e =>
        {
            var evt = (DomainEvents.DocumentVersionAttachedEvent)e;
            return new DocumentVersionAttachedEventSchema(
                evt.DocumentId.Value,
                evt.VersionRef.Value,
                evt.AttachedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentActivatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentActivatedEvent)e;
            return new DocumentActivatedEventSchema(evt.DocumentId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentArchivedEvent", e =>
        {
            var evt = (DomainEvents.DocumentArchivedEvent)e;
            return new DocumentArchivedEventSchema(evt.DocumentId.Value, evt.ArchivedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentRestoredEvent", e =>
        {
            var evt = (DomainEvents.DocumentRestoredEvent)e;
            return new DocumentRestoredEventSchema(evt.DocumentId.Value, evt.RestoredAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentSupersededEvent", e =>
        {
            var evt = (DomainEvents.DocumentSupersededEvent)e;
            return new DocumentSupersededEventSchema(
                evt.DocumentId.Value,
                evt.SupersedingDocumentId.Value,
                evt.SupersededAt.Value);
        });
    }
}
