using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Template;
using DomainEvents = Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

namespace Whycespace.Runtime.EventFabric.DomainSchemas;

/// <summary>
/// Schema module for the content/document/core-object/template BC. Owns
/// binding from domain event CLR types to the <see cref="EventSchemaRegistry"/>
/// plus outbound payload mappers that project domain events into shared
/// schema records for the projection layer.
/// </summary>
public sealed class ContentDocumentCoreObjectTemplateSchemaModule : ISchemaModule
{
    public void Register(ISchemaSink sink)
    {
        sink.RegisterSchema("DocumentTemplateCreatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentTemplateCreatedEvent), typeof(DocumentTemplateCreatedEventSchema));
        sink.RegisterSchema("DocumentTemplateUpdatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentTemplateUpdatedEvent), typeof(DocumentTemplateUpdatedEventSchema));
        sink.RegisterSchema("DocumentTemplateActivatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentTemplateActivatedEvent), typeof(DocumentTemplateActivatedEventSchema));
        sink.RegisterSchema("DocumentTemplateDeprecatedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentTemplateDeprecatedEvent), typeof(DocumentTemplateDeprecatedEventSchema));
        sink.RegisterSchema("DocumentTemplateArchivedEvent", EventVersion.Default,
            typeof(DomainEvents.DocumentTemplateArchivedEvent), typeof(DocumentTemplateArchivedEventSchema));

        sink.RegisterPayloadMapper("DocumentTemplateCreatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentTemplateCreatedEvent)e;
            return new DocumentTemplateCreatedEventSchema(
                evt.TemplateId.Value,
                evt.Name.Value,
                evt.Type.ToString(),
                evt.SchemaRef?.Value,
                evt.CreatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentTemplateUpdatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentTemplateUpdatedEvent)e;
            return new DocumentTemplateUpdatedEventSchema(
                evt.TemplateId.Value,
                evt.PreviousName.Value,
                evt.NewName.Value,
                evt.PreviousType.ToString(),
                evt.NewType.ToString(),
                evt.NewSchemaRef?.Value,
                evt.UpdatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentTemplateActivatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentTemplateActivatedEvent)e;
            return new DocumentTemplateActivatedEventSchema(evt.TemplateId.Value, evt.ActivatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentTemplateDeprecatedEvent", e =>
        {
            var evt = (DomainEvents.DocumentTemplateDeprecatedEvent)e;
            return new DocumentTemplateDeprecatedEventSchema(
                evt.TemplateId.Value,
                evt.Reason,
                evt.DeprecatedAt.Value);
        });
        sink.RegisterPayloadMapper("DocumentTemplateArchivedEvent", e =>
        {
            var evt = (DomainEvents.DocumentTemplateArchivedEvent)e;
            return new DocumentTemplateArchivedEventSchema(evt.TemplateId.Value, evt.ArchivedAt.Value);
        });
    }
}
