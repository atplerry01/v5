using Whycespace.Shared.Contracts.Content.Document.CoreObject.Document;
using Whycespace.Shared.Contracts.Events.Content.Document.CoreObject.Document;

namespace Whycespace.Projections.Content.Document.CoreObject.Document.Reducer;

public static class DocumentProjectionReducer
{
    public static DocumentReadModel Apply(DocumentReadModel state, DocumentCreatedEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            Title = e.Title,
            Type = e.Type,
            Classification = e.Classification,
            StructuralOwnerId = e.StructuralOwnerId,
            BusinessOwnerKind = e.BusinessOwnerKind,
            BusinessOwnerId = e.BusinessOwnerId,
            CurrentVersionId = null,
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static DocumentReadModel Apply(DocumentReadModel state, DocumentMetadataUpdatedEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            Title = e.NewTitle,
            Type = e.NewType,
            Classification = e.NewClassification,
            LastModifiedAt = e.UpdatedAt
        };

    public static DocumentReadModel Apply(DocumentReadModel state, DocumentVersionAttachedEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            CurrentVersionId = e.VersionId,
            LastModifiedAt = e.AttachedAt
        };

    public static DocumentReadModel Apply(DocumentReadModel state, DocumentActivatedEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            Status = "Active",
            LastModifiedAt = e.ActivatedAt
        };

    public static DocumentReadModel Apply(DocumentReadModel state, DocumentArchivedEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            Status = "Archived",
            LastModifiedAt = e.ArchivedAt
        };

    public static DocumentReadModel Apply(DocumentReadModel state, DocumentRestoredEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            Status = "Active",
            LastModifiedAt = e.RestoredAt
        };

    public static DocumentReadModel Apply(DocumentReadModel state, DocumentSupersededEventSchema e) =>
        state with
        {
            DocumentId = e.AggregateId,
            Status = "Superseded",
            LastModifiedAt = e.SupersededAt
        };
}
