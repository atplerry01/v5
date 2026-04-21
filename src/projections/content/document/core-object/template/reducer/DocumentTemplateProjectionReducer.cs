using Whycespace.Shared.Contracts.Content.Document.CoreObject.Template;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Template;

namespace Whycespace.Projections.Content.Document.CoreObject.Template.Reducer;

public static class DocumentTemplateProjectionReducer
{
    public static DocumentTemplateReadModel Apply(DocumentTemplateReadModel state, DocumentTemplateCreatedEventSchema e) =>
        state with
        {
            TemplateId = e.AggregateId,
            Name = e.Name,
            Type = e.Type,
            SchemaRefId = e.SchemaRefId,
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static DocumentTemplateReadModel Apply(DocumentTemplateReadModel state, DocumentTemplateUpdatedEventSchema e) =>
        state with
        {
            TemplateId = e.AggregateId,
            Name = e.NewName,
            Type = e.NewType,
            SchemaRefId = e.NewSchemaRefId,
            LastModifiedAt = e.UpdatedAt
        };

    public static DocumentTemplateReadModel Apply(DocumentTemplateReadModel state, DocumentTemplateActivatedEventSchema e) =>
        state with
        {
            TemplateId = e.AggregateId,
            Status = "Active",
            LastModifiedAt = e.ActivatedAt
        };

    public static DocumentTemplateReadModel Apply(DocumentTemplateReadModel state, DocumentTemplateDeprecatedEventSchema e) =>
        state with
        {
            TemplateId = e.AggregateId,
            Status = "Deprecated",
            LastModifiedAt = e.DeprecatedAt
        };

    public static DocumentTemplateReadModel Apply(DocumentTemplateReadModel state, DocumentTemplateArchivedEventSchema e) =>
        state with
        {
            TemplateId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };
}
